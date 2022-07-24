linux: /var/lib/pgsql/data
Windows C:\Program Files\PostgreSQL\[version_number]\data

pg_hba.conf
-----------
local   all             all                                     trust
host    all             all             127.0.0.1/32            trust
host    all             all             ::1/128                 trust
