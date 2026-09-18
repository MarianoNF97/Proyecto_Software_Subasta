#!/bin/bash
# ==============================================================================
# TEST DE ROLLBACK TRANSACCIONAL + INTEGRIDAD DEL LEDGER — SubastaYa
# ==============================================================================
# Demuestra que:
# 1. Un intento de puja con saldo insuficiente NO altera el saldo del usuario.
# 2. La ecuación Disponible = Total - Retenido se mantiene siempre.
# 3. Cada operación financiera genera un registro en el Ledger.
# ==============================================================================

BASE_URL="http://localhost:5094/api"

echo "============================================"
echo " SubastaYa — Test de Rollback + Ledger"
echo "============================================"
echo ""

# Registrar usuario
TS=$(date +%s)
USER=$(curl -s -X POST "$BASE_URL/auth/register" \
  -H "Content-Type: application/json" \
  -d "{\"email\":\"rollback_${TS}@test.com\",\"password\":\"Test1234\",\"nombreCompleto\":\"User Rollback\"}")
TOKEN=$(echo $USER | jq -r '.token')

# Depositar exactamente $500
curl -s -X POST "$BASE_URL/wallets/deposit" \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer $TOKEN" \
  -d '{"amount": 500}' > /dev/null

echo "[1/4] Usuario registrado con $500 de saldo."

# Saldo ANTES del intento fallido
echo ""
echo "=== Saldo ANTES del intento fallido ==="
BALANCE_BEFORE=$(curl -s "$BASE_URL/wallets/my-balance" \
  -H "Authorization: Bearer $TOKEN")
echo $BALANCE_BEFORE | jq .

TOTAL_BEFORE=$(echo $BALANCE_BEFORE | jq '.totalBalance')
TX_BEFORE=$(curl -s "$BASE_URL/wallets/my-transactions" \
  -H "Authorization: Bearer $TOKEN" | jq 'length')
echo "   Transacciones registradas: $TX_BEFORE"

# Intentar pujar con monto absurdo (debería fallar con 400)
echo ""
echo "[2/4] Intentando puja con saldo insuficiente ($999999)..."
HTTP_CODE=$(curl -s -o /dev/null -w "%{http_code}" -X POST "$BASE_URL/bids" \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer $TOKEN" \
  -d '{"auctionId": 1, "amount": 999999}')
echo "   Código HTTP recibido: $HTTP_CODE"

# Saldo DESPUÉS del intento fallido
echo ""
echo "=== Saldo DESPUÉS del intento fallido ==="
BALANCE_AFTER=$(curl -s "$BASE_URL/wallets/my-balance" \
  -H "Authorization: Bearer $TOKEN")
echo $BALANCE_AFTER | jq .

TOTAL_AFTER=$(echo $BALANCE_AFTER | jq '.totalBalance')
LOCKED_AFTER=$(echo $BALANCE_AFTER | jq '.lockedBalance')
AVAILABLE_AFTER=$(echo $BALANCE_AFTER | jq '.availableBalance')

TX_AFTER=$(curl -s "$BASE_URL/wallets/my-transactions" \
  -H "Authorization: Bearer $TOKEN" | jq 'length')
echo "   Transacciones registradas: $TX_AFTER"

# Verificar ecuación de balance
echo ""
echo "[3/4] Verificación de ecuación de integridad:"
CALCULATED=$(awk "BEGIN {print $TOTAL_AFTER - $LOCKED_AFTER}")
echo "   Total:              $TOTAL_AFTER"
echo "   Retenido:           $LOCKED_AFTER"
echo "   Disponible (API):   $AVAILABLE_AFTER"
echo "   Disponible (calc):  $CALCULATED"

echo ""
echo "[4/4] RESULTADOS:"
echo "============================================"

# Verificar rollback
if [ "$TOTAL_BEFORE" == "$TOTAL_AFTER" ]; then
  echo "   ✅ ROLLBACK VERIFICADO: Saldo intacto ($TOTAL_AFTER)"
else
  echo "   ❌ ERROR: Saldo cambió de $TOTAL_BEFORE a $TOTAL_AFTER"
fi

# Verificar que no se crearon transacciones espurias
if [ "$TX_BEFORE" == "$TX_AFTER" ]; then
  echo "   ✅ LEDGER LIMPIO: No se crearon transacciones espurias ($TX_AFTER registros)"
else
  echo "   ❌ ERROR: Se crearon transacciones ($TX_BEFORE → $TX_AFTER)"
fi

# Verificar ecuación
AVAILABLE_INT=$(echo "$AVAILABLE_AFTER" | awk '{printf "%d", $1}')
CALCULATED_INT=$(echo "$CALCULATED" | awk '{printf "%d", $1}')
if [ "$AVAILABLE_INT" == "$CALCULATED_INT" ]; then
  echo "   ✅ ECUACIÓN VÁLIDA: Disponible = Total - Retenido"
else
  echo "   ❌ ERROR: Disponible ($AVAILABLE_AFTER) ≠ Total-Retenido ($CALCULATED)"
fi

echo "============================================"

