CREATE DATABASE IF NOT EXISTS auth_db;
CREATE USER 'auth'@'%' IDENTIFIED BY '12345';
GRANT ALL PRIVILEGES ON auth_db.* TO 'auth'@'%';

CREATE DATABASE IF NOT EXISTS account_db;
CREATE USER 'account'@'%' IDENTIFIED BY '12345';
GRANT ALL PRIVILEGES ON account_db.* TO 'account'@'%';

CREATE DATABASE IF NOT EXISTS users_db;
CREATE USER 'users'@'%' IDENTIFIED BY '12345';
GRANT ALL PRIVILEGES ON users_db.* TO 'users'@'%';

CREATE DATABASE IF NOT EXISTS session_db;
CREATE USER 'session'@'%' IDENTIFIED BY '12345';
GRANT ALL PRIVILEGES ON session_db.* TO 'session'@'%';