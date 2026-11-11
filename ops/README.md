# ops — Backup automático de la BD de producción

Copia diaria de la BD `moneytracker` (MariaDB) en el servidor, con un **timer de
systemd** en vez de cron: si el servidor estaba apagado a la hora prevista la
copia se hace al arrancar (`Persistent=true`), y los fallos quedan en el journal
en vez de en un correo que nadie lee.

| Fichero | Qué es |
|---|---|
| `backup-moneytracker.sh` | El dump: comprime, comprueba que no salió cortado y borra lo viejo |
| `moneytracker-backup.service` | La tarea (oneshot, como root) |
| `moneytracker-backup.timer` | Cuándo: todos los días a las 03:30 |

**Dónde acaban las copias:** `/var/backups/moneytracker/` (permisos 700, cada
copia 600). Se guardan **30 diarias** y, del día 1 de cada mes, una copia en
`mensuales/` que aguanta algo más de un año. Un dump de esta BD ocupa unos 30 KB
comprimido, así que la retención larga sale gratis.

**Sin contraseñas en ningún fichero**: el root de MariaDB entra por `unix_socket`,
así que basta con que la tarea corra como root.

## Instalar (una vez, en el servidor)

Desde una copia del repo en el servidor:

```bash
sudo install -m 750 ops/backup-moneytracker.sh /usr/local/bin/backup-moneytracker.sh
sudo install -m 644 ops/moneytracker-backup.service /etc/systemd/system/
sudo install -m 644 ops/moneytracker-backup.timer /etc/systemd/system/
sudo systemctl daemon-reload
sudo systemctl enable --now moneytracker-backup.timer
```

## Comprobar que funciona

```bash
sudo systemctl start moneytracker-backup.service     # lanza una copia ahora
sudo journalctl -u moneytracker-backup.service -n 20 # tiene que decir "Backup OK"
sudo ls -lh /var/backups/moneytracker/
systemctl list-timers moneytracker-backup.timer      # cuándo toca la siguiente
```

Para ver que la copia sirve de verdad (no solo que existe):

```bash
sudo gzip -dc /var/backups/moneytracker/moneytracker-*.sql.gz | grep -c "INSERT INTO"
```

## Restaurar

```bash
sudo gzip -dc /var/backups/moneytracker/moneytracker-20260917_033000.sql.gz | sudo mariadb moneytracker
```

## Lo que esto NO resuelve

Las copias viven **en el mismo disco que la BD**: sirven para un borrado o una
migración que sale mal, no para que el servidor se muera. Cuando quieras cerrar
eso, lo suyo es que algo tire de ellas hacia fuera, por ejemplo desde tu equipo:

```bash
scp josema@<host>:/var/backups/moneytracker/moneytracker-*.sql.gz ./backups/
```

## Opcional: copia antes de cada despliegue

El momento con más riesgo es un despliegue que trae migraciones de DbUp. Para
hacer una copia justo antes, añade esto al principio del paso de la release:

```bash
sudo /usr/local/bin/backup-moneytracker.sh
```

Como el agente no corre como root, hace falta darle permiso **solo para ese
script** (`sudo visudo -f /etc/sudoers.d/moneytracker-backup`):

```
<usuario-del-agente> ALL=(root) NOPASSWD: /usr/local/bin/backup-moneytracker.sh
```

## Copia por correo (saca el backup del servidor)

El backup pesa unos 9 KB comprimido, así que la copia diaria se manda **adjunta
por correo**. Es lo que lo saca del servidor: si la máquina muere, la copia
sigue estando en tu buzón.

Hace falta `msmtp` (cliente de correo mínimo) y una cuenta por la que salir.
Mandar el correo "desde el servidor" sin cuenta no funciona: Gmail y Microsoft
rechazan o mandan a spam lo que viene de una IP de VPS sin reputación.

```bash
sudo apt install msmtp
sudo install -m 600 -o root -g root ops/msmtprc.example /etc/msmtprc
sudo nano /etc/msmtprc                       # cuenta + contraseña de aplicación
sudo install -m 644 ops/moneytracker-backup.env.example /etc/default/moneytracker-backup
sudo nano /etc/default/moneytracker-backup   # a qué correo llega
sudo systemctl restart moneytracker-backup.timer
sudo systemctl start moneytracker-backup.service    # prueba: debería llegarte el mail
```

Con **Gmail** no vale la contraseña de la cuenta: hay que tener la verificación
en dos pasos y generar una **contraseña de aplicación** de 16 letras en
<https://myaccount.google.com/apppasswords>.

**Qué llega:** un correo al día con asunto `MoneyTracker: backup del <fecha>`,
el `.sql.gz` adjunto y, en el cuerpo, el tamaño y **cuántas tablas e INSERTs
lleva dentro** — que es lo que distingue una copia buena de un fichero vacío.
Si el backup falla, llega un correo avisando en vez del backup.

**Sin configurar el correo, todo esto se salta**: la copia se hace igual y se
queda en el servidor. Que no se pueda mandar el correo tampoco invalida un
backup que ya está en disco; queda el aviso en el journal y en
`/var/log/msmtp.log`.

⚠️ `/etc/msmtprc` lleva una contraseña: permisos 600 y de root. Al repo solo
sube la plantilla, sin rellenar.
