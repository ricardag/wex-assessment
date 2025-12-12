#!/bin/bash
# This script grants specific permissions to the application user
# MariaDB entrypoint creates the user but grants ALL PRIVILEGES
# We want to restrict to only SELECT, INSERT, UPDATE, DELETE

mariadb -uroot -p"${MARIADB_ROOT_PASSWORD}" <<-EOSQL
    -- First revoke all privileges (ignore error if none exist)
    REVOKE ALL PRIVILEGES ON ${MARIADB_DATABASE}.* FROM '${MARIADB_USER}'@'%';
    -- Grant only specific privileges
    GRANT SELECT, INSERT, UPDATE, DELETE ON ${MARIADB_DATABASE}.* TO '${MARIADB_USER}'@'%';
    FLUSH PRIVILEGES;
EOSQL

# Check if the command succeeded
if [ $? -ne 0 ]; then
    echo "Note: REVOKE failed (user may not have had privileges), but will continue with GRANT"
    # Try just granting without revoking
    mariadb -uroot -p"${MARIADB_ROOT_PASSWORD}" <<-EOSQL
        GRANT SELECT, INSERT, UPDATE, DELETE ON ${MARIADB_DATABASE}.* TO '${MARIADB_USER}'@'%';
        FLUSH PRIVILEGES;
EOSQL
fi
