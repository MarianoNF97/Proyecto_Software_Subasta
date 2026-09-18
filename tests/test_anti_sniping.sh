#!/bin/bash
# ==============================================================================
# TEST DE ANTI-SNIPING — SubastaYa
# ==============================================================================
# Demuestra que la regla anti-sniping extiende la subasta 2 minutos
# cuando una puja ocurre en los últimos 60 segundos.
# ==============================================================================

BASE_URL="http://localhost:5094/api"

echo "============================================"
echo " SubastaYa — Test de Anti-Sniping"
echo "============================================"
echo ""

# Registrar vendedor y comprador
TS=$(date +%s)
VENDOR=$(curl -s -X POST "$BASE_URL/auth/register" \
  -H "Content-Type: application/json" \
  -d "{\"email\":\"snipe_vendor_${TS}@test.com\",\"password\":\"Test1234\",\"nombreCompleto\":\"Vendedor Snipe\"}")
VENDOR_TOKEN=$(echo $VENDOR | jq -r '.token')

BUYER=$(curl -s -X POST "$BASE_URL/auth/register" \
  -H "Content-Type: application/json" \
  -d "{\"email\":\"snipe_buyer_${TS}@test.com\",\"password\":\"Test1234\",\"nombreCompleto\":\"Comprador Snipe\"}")
BUYER_TOKEN=$(echo $BUYER | jq -r '.token')

# Depositar fondos al comprador
curl -s -X POST "$BASE_URL/wallets/deposit" \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer $BUYER_TOKEN" \
  -d '{"amount": 50000}' > /dev/null

echo "[1/5] Usuarios registrados y fondeados."

# Crear subasta que cierra en 30 segundos
START_DATE=$(date -u -d '-2 minutes' +%Y-%m-%dT%H:%M:%SZ 2>/dev/null || date -u -v-2M +%Y-%m-%dT%H:%M:%SZ)
END_DATE=$(date -u -d '+30 seconds' +%Y-%m-%dT%H:%M:%SZ 2>/dev/null || date -u -v+30S +%Y-%m-%dT%H:%M:%SZ)

AUCTION=$(curl -s -X POST "$BASE_URL/auctions" \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer $VENDOR_TOKEN" \
  -d "{
    \"title\": \"Test Anti-Sniping ${TS}\",
    \"description\": \"Cierra en 30 segundos para probar anti-sniping\",
    \"categoryId\": 1,
    \"imageUrl\": \"\",
    \"startingPrice\": 50,
    \"minIncrement\": 5,
    \"startDate\": \"$START_DATE\",
    \"endDate\": \"$END_DATE\"
  }")
AUCTION_ID=$(echo $AUCTION | jq -r '.id')
echo "[2/5] Subasta creada con ID: $AUCTION_ID (cierra en ~30 segundos)"

# Esperar activación
echo "[3/5] Esperando activación del Worker (15 seg)..."
sleep 15

# Fecha fin ANTES de la puja
echo ""
echo "=== Fecha fin ANTES de la puja ==="
BEFORE=$(curl -s "$BASE_URL/auctions/$AUCTION_ID" | jq -r '.endDate')
echo "   $BEFORE"

# Pujar dentro de la zona crítica (últimos 60 segundos)
echo ""
echo "[4/5] Pujando en zona crítica (últimos segundos)..."
BID_RESULT=$(curl -s -X POST "$BASE_URL/bids" \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer $BUYER_TOKEN" \
  -d "{\"auctionId\": $AUCTION_ID, \"amount\": 55}")

TIME_EXTENDED=$(echo $BID_RESULT | jq -r '.timeExtended')
NEW_END=$(echo $BID_RESULT | jq -r '.newEndDate')
echo "   timeExtended: $TIME_EXTENDED"
echo "   newEndDate:    $NEW_END"

# Fecha fin DESPUÉS de la puja
echo ""
echo "=== Fecha fin DESPUÉS de la puja ==="
AFTER=$(curl -s "$BASE_URL/auctions/$AUCTION_ID" | jq -r '.endDate')
echo "   $AFTER"

echo ""
echo "[5/5] RESULTADO:"
if [ "$TIME_EXTENDED" == "true" ]; then
  echo "   ✅ ANTI-SNIPING VERIFICADO: La subasta fue extendida 2 minutos."
  echo "   Antes: $BEFORE"
  echo "   Después: $AFTER"
else
  echo "   ⚠️  Anti-sniping NO se activó. Verificar que la puja cayó en los últimos 60 seg."
fi
echo "============================================"

