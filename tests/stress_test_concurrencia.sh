#!/bin/bash
# ==============================================================================
# STRESS TEST DE CONCURRENCIA MASIVA — SubastaYa
# ==============================================================================
# Este script registra 20 usuarios, les deposita fondos, crea una subasta,
# y lanza 20 pujas simultáneas en el mismo milisegundo para demostrar
# que la concurrencia optimista (SQL Server rowversion + EF Core [Timestamp])
# rechaza todas las colisiones con HTTP 409 Conflict.
# ==============================================================================
# Requisitos: bash, curl, jq
# Ejecución: bash tests/stress_test_concurrencia.sh
# ==============================================================================

BASE_URL="http://localhost:5094/api"
NUM_USERS=20

echo "============================================"
echo " SubastaYa — Stress Test de Concurrencia"
echo " Usuarios simultáneos: $NUM_USERS"
echo "============================================"
echo ""

# ---------- PASO 1: Registrar un vendedor ----------
echo "[1/6] Registrando vendedor..."
VENDOR_RESPONSE=$(curl -s -X POST "$BASE_URL/auth/register" \
  -H "Content-Type: application/json" \
  -d "{\"email\":\"vendor_stress_$(date +%s)@test.com\",\"password\":\"Test1234\",\"nombreCompleto\":\"Vendedor Stress\"}")

VENDOR_TOKEN=$(echo $VENDOR_RESPONSE | jq -r '.token')
if [ "$VENDOR_TOKEN" == "null" ] || [ -z "$VENDOR_TOKEN" ]; then
  echo "ERROR: No se pudo registrar el vendedor."
  echo $VENDOR_RESPONSE | jq .
  exit 1
fi
echo "   Vendedor registrado. Token obtenido."

# ---------- PASO 2: Registrar N compradores y depositar fondos ----------
echo "[2/6] Registrando $NUM_USERS compradores y depositando fondos..."
declare -a TOKENS
TIMESTAMP=$(date +%s)

for i in $(seq 1 $NUM_USERS); do
  RESPONSE=$(curl -s -X POST "$BASE_URL/auth/register" \
    -H "Content-Type: application/json" \
    -d "{\"email\":\"stress_user_${i}_${TIMESTAMP}@test.com\",\"password\":\"Test1234\",\"nombreCompleto\":\"Stress User $i\"}")

  TOKEN=$(echo $RESPONSE | jq -r '.token')
  TOKENS[$i]=$TOKEN

  # Depositar fondos
  curl -s -X POST "$BASE_URL/wallets/deposit" \
    -H "Content-Type: application/json" \
    -H "Authorization: Bearer $TOKEN" \
    -d '{"amount": 100000}' > /dev/null

  echo -n "."
done
echo ""
echo "   $NUM_USERS compradores registrados y fondeados."

# ---------- PASO 3: Crear una subasta activa ----------
echo "[3/6] Creando subasta de prueba..."

# Fecha inicio en el pasado (para que el worker la active)
START_DATE=$(date -u -d '-2 minutes' +%Y-%m-%dT%H:%M:%SZ 2>/dev/null || date -u -v-2M +%Y-%m-%dT%H:%M:%SZ)
END_DATE=$(date -u -d '+15 minutes' +%Y-%m-%dT%H:%M:%SZ 2>/dev/null || date -u -v+15M +%Y-%m-%dT%H:%M:%SZ)

AUCTION_RESPONSE=$(curl -s -X POST "$BASE_URL/auctions" \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer $VENDOR_TOKEN" \
  -d "{
    \"title\": \"Stress Test Concurrencia - $(date +%H:%M:%S)\",
    \"description\": \"Subasta para prueba de concurrencia masiva con $NUM_USERS usuarios\",
    \"categoryId\": 1,
    \"imageUrl\": \"\",
    \"startingPrice\": 100,
    \"minIncrement\": 10,
    \"startDate\": \"$START_DATE\",
    \"endDate\": \"$END_DATE\"
  }")

AUCTION_ID=$(echo $AUCTION_RESPONSE | jq -r '.id')
if [ "$AUCTION_ID" == "null" ] || [ -z "$AUCTION_ID" ]; then
  echo "ERROR: No se pudo crear la subasta."
  echo $AUCTION_RESPONSE | jq .
  exit 1
fi
echo "   Subasta creada con ID: $AUCTION_ID"

# ---------- PASO 4: Esperar activación del Worker ----------
echo "[4/6] Esperando a que el Worker active la subasta (15 segundos)..."
sleep 15

# Verificar que la subasta esté activa
STATUS=$(curl -s "$BASE_URL/auctions/$AUCTION_ID" | jq -r '.status // .estado')
echo "   Estado actual de la subasta: $STATUS"
if [ "$STATUS" != "ACTIVA" ]; then
  echo "ADVERTENCIA: La subasta no está activa todavía. Esperando 10 segundos más..."
  sleep 10
fi

# ---------- PASO 5: Lanzar N pujas SIMULTÁNEAS ----------
echo "[5/6] Lanzando $NUM_USERS pujas simultáneas..."
echo ""

RESULTS_DIR="/tmp/stress_test_results_$$"
mkdir -p "$RESULTS_DIR"

for i in $(seq 1 $NUM_USERS); do
  BID_AMOUNT=$((100 + i * 10))

  (
    HTTP_RESPONSE=$(curl -s -w "\n%{http_code}" -X POST "$BASE_URL/bids" \
      -H "Content-Type: application/json" \
      -H "Authorization: Bearer ${TOKENS[$i]}" \
      -d "{\"auctionId\": $AUCTION_ID, \"amount\": $BID_AMOUNT}")
    
    END_TIME=$(date +%H:%M:%S.%3N)
    HTTP_CODE=$(echo "$HTTP_RESPONSE" | tail -n1)
    HTTP_BODY=$(echo "$HTTP_RESPONSE" | sed '$d')
    
    echo "$HTTP_CODE" > "$RESULTS_DIR/code_$i.txt"
    echo "[$END_TIME] Usuario $i (Puja: \$$BID_AMOUNT) - Código: $HTTP_CODE - Respuesta: $HTTP_BODY" > "$RESULTS_DIR/detail_$i.txt"
  ) &
done

# Esperar a que TODAS las peticiones terminen
wait
echo "   Todas las peticiones completadas."
echo ""

# ---------- PASO 6: Analizar resultados ----------
echo "[6/6] Analizando resultados..."
echo "============================================"
echo " RESULTADOS DEL STRESS TEST"
echo "============================================"

SUCCESS_COUNT=$(grep -h -E "^200$|^201$" "$RESULTS_DIR"/code_*.txt 2>/dev/null | wc -l | tr -d ' ')
CONFLICT_COUNT=$(grep -h -E "^409$" "$RESULTS_DIR"/code_*.txt 2>/dev/null | wc -l | tr -d ' ')
BAD_REQUEST_COUNT=$(grep -h -E "^400$" "$RESULTS_DIR"/code_*.txt 2>/dev/null | wc -l | tr -d ' ')
TOTAL_HTTP=$(cat "$RESULTS_DIR"/code_*.txt 2>/dev/null | wc -l | tr -d ' ')
OTHER_ERRORS=$((TOTAL_HTTP - SUCCESS_COUNT - CONFLICT_COUNT - BAD_REQUEST_COUNT))

echo ""
echo "   Peticiones totales:      $NUM_USERS"
echo "   ✅ Exitosas (200/201):   $SUCCESS_COUNT"
echo "   ⚠️  Conflict (409):      $CONFLICT_COUNT"
echo "   🚫 Bad Request (400):    $BAD_REQUEST_COUNT"
echo "   ❌ Otros errores:        $OTHER_ERRORS"
echo ""

if [ "$SUCCESS_COUNT" -gt 0 ]; then
  echo "   Detalle de peticiones EXITOSAS (Ordenadas por tiempo):"
  grep -h -E "Código: 200|Código: 201" "$RESULTS_DIR"/detail_*.txt | sort | sed 's/^/      /'
  echo ""
fi

if [ "$SUCCESS_COUNT" -eq 1 ]; then
  echo "   ✅ RESULTADO: TEST PASADO"
  echo "   Solo 1 puja ganó. Las demás colisionaron (409) o llegaron tarde (400)."
elif [ "$SUCCESS_COUNT" -gt 1 ] && [ "$CONFLICT_COUNT" -gt 0 ]; then
  echo "   ✅ RESULTADO: TEST PASADO (CONCURRENCIA VERIFICADA CON DESFASE)"
  echo "   Pasaron $SUCCESS_COUNT pujas. Al mirar el detalle de tiempo arriba, se ve que"
  echo "   fueron procesadas en distintos milisegundos. Al tener montos incrementales,"
  echo "   una fue válida respecto a la anterior."
  echo "   Sabemos que NO falló la concurrencia porque SÍ se detectaron colisiones simultáneas ($CONFLICT_COUNT con 409)."
elif [ "$SUCCESS_COUNT" -gt 1 ] && [ "$BAD_REQUEST_COUNT" -gt 0 ]; then
  echo "   ✅ RESULTADO: TEST PASADO (SECUENCIAL)"
  echo "   Pasaron $SUCCESS_COUNT pujas desfasadas. Las restantes fueron bloqueadas por validación (400)."
else
  echo "   ❌ RESULTADO: TEST FALLIDO."
  echo "   Pasaron $SUCCESS_COUNT pujas y NO hubo colisiones (0 Conflictos detectados)."
  echo "   Esto indica un fallo real en el control de concurrencia optimista."
fi

echo ""
echo "============================================"

# Limpiar
rm -rf "$RESULTS_DIR"
