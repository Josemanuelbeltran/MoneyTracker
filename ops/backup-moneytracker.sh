#!/bin/bash
# Backup de la BD de produccion de MoneyTracker (MariaDB).
# Lo lanza el timer de systemd moneytracker-backup.timer, como root:
# el root de MariaDB entra por unix_socket, asi que no hay contraseña que guardar.
#
# Si /etc/default/moneytracker-backup define MAIL_TO, manda la copia adjunta por
# correo: es lo que la saca del servidor, que es el unico disco donde vive.
set -euo pipefail

DB=moneytracker
DEST=/var/backups/moneytracker
KEEP_DAILY_DAYS=30
KEEP_MONTHLY_DAYS=400

MAIL_TO="${MAIL_TO:-}"
MAIL_FROM="${MAIL_FROM:-$MAIL_TO}"
MSMTP_CONFIG=/etc/msmtprc

# MariaDB 11 renombro la herramienta; mysqldump suele seguir existiendo como enlace.
if command -v mariadb-dump >/dev/null 2>&1; then
  DUMP=mariadb-dump
else
  DUMP=mysqldump
fi

# ---------------------------------------------------------------- correo ------

# Se construye el MIME a mano y se manda por msmtp, para no instalar mutt ni
# nada mas: base64 ya viene con el sistema.
enviar_correo() {
  local asunto="$1" cuerpo="$2" adjunto="${3:-}"

  if [ -z "$MAIL_TO" ]; then
    return 0
  fi
  if ! command -v msmtp >/dev/null 2>&1; then
    echo "MAIL_TO esta puesto pero msmtp no esta instalado: no se manda correo" >&2
    return 0
  fi

  local frontera="=_moneytracker_$$_$(date +%s)"
  {
    echo "From: $MAIL_FROM"
    echo "To: $MAIL_TO"
    echo "Subject: $asunto"
    echo "MIME-Version: 1.0"

    if [ -n "$adjunto" ]; then
      local nombre
      nombre=$(basename "$adjunto")
      echo "Content-Type: multipart/mixed; boundary=\"$frontera\""
      echo
      echo "--$frontera"
      echo "Content-Type: text/plain; charset=utf-8"
      echo
      echo "$cuerpo"
      echo
      echo "--$frontera"
      echo "Content-Type: application/gzip; name=\"$nombre\""
      echo "Content-Transfer-Encoding: base64"
      echo "Content-Disposition: attachment; filename=\"$nombre\""
      echo
      base64 "$adjunto"
      echo "--$frontera--"
    else
      echo "Content-Type: text/plain; charset=utf-8"
      echo
      echo "$cuerpo"
    fi
  } | msmtp --file="$MSMTP_CONFIG" "$MAIL_TO" || {
    # Que no se pueda avisar no invalida el backup, que ya esta en disco.
    echo "El backup esta hecho pero el correo no salio (mira /var/log/msmtp.log)" >&2
    return 0
  }
}

# Cualquier fallo de aqui en adelante avisa por correo antes de morir.
avisar_fallo() {
  trap - ERR
  enviar_correo \
    "MoneyTracker: FALLO el backup de $(date +%d-%m-%Y)" \
    "El backup de la BD $DB no se ha podido hacer.

Para ver por que:
  sudo journalctl -u moneytracker-backup.service -n 30"
}
trap avisar_fallo ERR

# ---------------------------------------------------------------- backup ------

stamp=$(date +%Y%m%d_%H%M%S)
out="$DEST/$DB-$stamp.sql.gz"
tmp="$DEST/.$DB-$stamp.sql.gz.part"

mkdir -p "$DEST/mensuales"
chmod 700 "$DEST"

# --single-transaction: copia coherente sin bloquear la app mientras escribe.
"$DUMP" --single-transaction --quick --routines --triggers --events \
  --default-character-set=utf8mb4 "$DB" | gzip -9 > "$tmp"

# Un dump cortado a la mitad tambien pesa y tambien parece un backup. Solo vale
# si trae la linea final que escribe la propia herramienta.
if ! gzip -dc "$tmp" | tail -5 | grep -q "Dump completed"; then
  rm -f "$tmp"
  echo "El dump de $DB salio incompleto: NO se guarda" >&2
  exit 1
fi

mv "$tmp" "$out"
chmod 600 "$out"

# El dia 1 se aparta una copia del mes, que aguanta mucho mas que las diarias.
if [ "$(date +%d)" = "01" ]; then
  cp -p "$out" "$DEST/mensuales/$DB-$(date +%Y%m).sql.gz"
fi

find "$DEST" -maxdepth 1 -name "$DB-*.sql.gz" -mtime "+$KEEP_DAILY_DAYS" -delete
find "$DEST/mensuales" -name "$DB-*.sql.gz" -mtime "+$KEEP_MONTHLY_DAYS" -delete

# ------------------------------------------------------- resumen y correo -----

tamano=$(du -h "$out" | cut -f1)
diarias=$(find "$DEST" -maxdepth 1 -name "$DB-*.sql.gz" | wc -l)
# Que el fichero exista no dice nada; lo que dice algo es que lleve datos dentro.
inserts=$(gzip -dc "$out" | grep -c "INSERT INTO" || true)
tablas=$(gzip -dc "$out" | grep -c "CREATE TABLE" || true)

echo "Backup OK: $out ($tamano), $diarias diarias guardadas"

enviar_correo \
  "MoneyTracker: backup del $(date +%d-%m-%Y) ($tamano)" \
  "Copia de la BD $DB, adjunta.

  Fichero:  $(basename "$out")
  Tamaño:   $tamano
  Tablas:   $tablas
  INSERTs:  $inserts

En el servidor se guardan $diarias copias diarias en $DEST
(30 dias) y una por mes en $DEST/mensuales.

Restaurar:
  sudo gzip -dc $(basename "$out") | sudo mariadb $DB" \
  "$out"
