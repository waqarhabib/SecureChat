CREATE DATABASE IF NOT EXISTS auth_db;
CREATE USER 'auth'@'%' IDENTIFIED BY '12345';
GRANT ALL PRIVILEGES ON auth_db.* TO 'auth'@'%';

CREATE DATABASE IF NOT EXISTS account_db;
CREATE USER 'account'@'%' IDENTIFIED BY '12345';
GRANT ALL PRIVILEGES ON account_db.* TO 'account'@'%';

CREATE DATABASE IF NOT EXISTS associations_db;
CREATE USER 'chat'@'%' IDENTIFIED BY '12345';
GRANT ALL PRIVILEGES ON chat_db.* TO 'chat'@'%';