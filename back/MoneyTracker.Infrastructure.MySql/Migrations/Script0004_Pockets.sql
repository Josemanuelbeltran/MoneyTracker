-- =========================================================
-- Script0004_Pockets
-- "Pockets": sub-saldos dentro de una cuenta (huchas/sobres).
-- El saldo del pocket es una asignación del saldo de la cuenta.
-- Aditivo y seguro (CREATE TABLE IF NOT EXISTS, no borra datos).
-- DbUp lo ejecuta una sola vez (tracking en SchemaVersions).
-- =========================================================

CREATE TABLE IF NOT EXISTS `Pockets` (
  `id` INT UNSIGNED NOT NULL AUTO_INCREMENT,
  `user_id` INT UNSIGNED NOT NULL,
  `account_id` INT UNSIGNED NOT NULL,
  `name` VARCHAR(100) NOT NULL,
  `balance` DECIMAL(10,2) NOT NULL DEFAULT 0.00,
  `created_at` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `updated_at` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
  PRIMARY KEY (`id`),
  KEY `idx_pocket_user` (`user_id`),
  KEY `idx_pocket_account` (`account_id`),
  CONSTRAINT `fk_pocket_user`    FOREIGN KEY (`user_id`)    REFERENCES `Users` (`id`)    ON DELETE CASCADE ON UPDATE CASCADE,
  CONSTRAINT `fk_pocket_account` FOREIGN KEY (`account_id`) REFERENCES `Accounts` (`id`) ON DELETE CASCADE ON UPDATE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;
