-- =========================================================
-- Script0003_Transfers
-- Transferencias entre cuentas del mismo usuario.
-- Aditivo y seguro (CREATE TABLE IF NOT EXISTS, no borra datos).
-- DbUp lo ejecuta una sola vez (tracking en SchemaVersions).
-- =========================================================

CREATE TABLE IF NOT EXISTS `Transfers` (
  `id` INT UNSIGNED NOT NULL AUTO_INCREMENT,
  `user_id` INT UNSIGNED NOT NULL,
  `from_account_id` INT UNSIGNED NOT NULL,
  `to_account_id` INT UNSIGNED NOT NULL,
  `amount` DECIMAL(10,2) NOT NULL,
  `transfer_date` DATETIME NOT NULL,
  `description` TEXT NULL,
  `created_at` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `updated_at` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
  PRIMARY KEY (`id`),
  KEY `idx_tr_user_date` (`user_id`,`transfer_date`),
  KEY `idx_tr_from` (`from_account_id`),
  KEY `idx_tr_to` (`to_account_id`),
  CONSTRAINT `fk_tr_user` FOREIGN KEY (`user_id`) REFERENCES `Users` (`id`) ON DELETE CASCADE ON UPDATE CASCADE,
  CONSTRAINT `fk_tr_from` FOREIGN KEY (`from_account_id`) REFERENCES `Accounts` (`id`) ON DELETE CASCADE ON UPDATE CASCADE,
  CONSTRAINT `fk_tr_to`   FOREIGN KEY (`to_account_id`)   REFERENCES `Accounts` (`id`) ON DELETE CASCADE ON UPDATE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;
