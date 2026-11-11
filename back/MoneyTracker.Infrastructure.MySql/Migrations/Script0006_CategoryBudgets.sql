-- =========================================================
-- Script0006_CategoryBudgets
-- Presupuesto mensual (recurrente) por categoría/subcategoría de gasto.
-- Un importe por (usuario, categoría), aplica a todos los meses.
-- Aditivo y seguro (CREATE TABLE IF NOT EXISTS). DbUp lo ejecuta una vez.
-- =========================================================

CREATE TABLE IF NOT EXISTS `CategoryBudgets` (
  `id` INT UNSIGNED NOT NULL AUTO_INCREMENT,
  `user_id` INT UNSIGNED NOT NULL,
  `category_id` INT UNSIGNED NOT NULL,
  `monthly_limit` DECIMAL(10,2) NOT NULL,
  `created_at` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `updated_at` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
  PRIMARY KEY (`id`),
  UNIQUE KEY `uq_catbudget_user_cat` (`user_id`,`category_id`),
  KEY `idx_catbudget_user` (`user_id`),
  CONSTRAINT `fk_catbudget_user`     FOREIGN KEY (`user_id`)     REFERENCES `Users` (`id`)              ON DELETE CASCADE ON UPDATE CASCADE,
  CONSTRAINT `fk_catbudget_category` FOREIGN KEY (`category_id`) REFERENCES `ExpenseCategories` (`id`) ON DELETE CASCADE ON UPDATE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;
