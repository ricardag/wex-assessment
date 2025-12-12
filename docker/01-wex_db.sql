

CREATE DATABASE IF NOT EXISTS `wex_db` DEFAULT CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci;
USE `wex_db`;

CREATE TABLE IF NOT EXISTS `Currency` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `country` varchar(256) NOT NULL,
  `currency` varchar(256) NOT NULL,
  PRIMARY KEY (`id`),
  UNIQUE KEY `Currency_country_IDX` (`country`,`currency`) USING BTREE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

CREATE TABLE IF NOT EXISTS `Purchases` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `description` varchar(50) NOT NULL,
  `transaction_datetime_utc` datetime NOT NULL,
  `purchase_amount` decimal(10,2) NOT NULL,
  `transaction_identifier` char(36) NOT NULL,
  PRIMARY KEY (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;
