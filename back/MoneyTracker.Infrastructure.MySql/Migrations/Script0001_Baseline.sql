-- =========================================================
-- Script0001_Baseline
-- Esquema base de MoneyTracker (idempotente: CREATE TABLE IF NOT EXISTS).
-- Refleja el esquema real ya existente. NO contiene DROPs: es seguro
-- ejecutarlo sobre una BD con datos (los CREATE se saltan si ya existen).
-- =========================================================

-- Users
CREATE TABLE IF NOT EXISTS `Users` (
  `id` INT UNSIGNED NOT NULL AUTO_INCREMENT,
  `name` VARCHAR(50) NOT NULL,
  `email` VARCHAR(255) NOT NULL,
  `password_hash` VARCHAR(255) NOT NULL,
  `phone_number` VARCHAR(15) NULL,
  `verified_phone` TINYINT(1) NOT NULL DEFAULT 0,
  `created_at` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `updated_at` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
  PRIMARY KEY (`id`),
  UNIQUE KEY `uq_users_email` (`email`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- AccountTypes
CREATE TABLE IF NOT EXISTS `AccountTypes` (
  `id` INT UNSIGNED NOT NULL AUTO_INCREMENT,
  `name` VARCHAR(50) NOT NULL,
  PRIMARY KEY (`id`),
  UNIQUE KEY `name` (`name`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- ExpenseCategories (jerárquica)
CREATE TABLE IF NOT EXISTS `ExpenseCategories` (
  `id` INT UNSIGNED NOT NULL AUTO_INCREMENT,
  `name` VARCHAR(100) NOT NULL,
  `parent_id` INT UNSIGNED NULL,
  `created_at` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `updated_at` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
  PRIMARY KEY (`id`),
  KEY `fk_expensecategories_parent` (`parent_id`),
  CONSTRAINT `fk_expensecategories_parent` FOREIGN KEY (`parent_id`) REFERENCES `ExpenseCategories` (`id`) ON DELETE CASCADE ON UPDATE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- IncomeCategories (jerárquica)
CREATE TABLE IF NOT EXISTS `IncomeCategories` (
  `id` INT UNSIGNED NOT NULL AUTO_INCREMENT,
  `name` VARCHAR(100) NOT NULL,
  `parent_id` INT UNSIGNED NULL,
  `created_at` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `updated_at` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
  PRIMARY KEY (`id`),
  KEY `fk_incomecategories_parent` (`parent_id`),
  CONSTRAINT `fk_incomecategories_parent` FOREIGN KEY (`parent_id`) REFERENCES `IncomeCategories` (`id`) ON DELETE CASCADE ON UPDATE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- Accounts
CREATE TABLE IF NOT EXISTS `Accounts` (
  `id` INT UNSIGNED NOT NULL AUTO_INCREMENT,
  `user_id` INT UNSIGNED NOT NULL,
  `account_type_id` INT UNSIGNED NOT NULL,
  `name` VARCHAR(100) NOT NULL,
  `balance` DECIMAL(10,2) NOT NULL DEFAULT 0.00,
  `created_at` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `updated_at` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
  PRIMARY KEY (`id`),
  KEY `fk_accounts_user` (`user_id`),
  KEY `fk_accounts_type` (`account_type_id`),
  CONSTRAINT `fk_accounts_user` FOREIGN KEY (`user_id`) REFERENCES `Users` (`id`) ON DELETE CASCADE ON UPDATE CASCADE,
  CONSTRAINT `fk_accounts_type` FOREIGN KEY (`account_type_id`) REFERENCES `AccountTypes` (`id`) ON DELETE RESTRICT ON UPDATE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- Expenses (nota: expense_date es DATETIME; no tiene created_at)
CREATE TABLE IF NOT EXISTS `Expenses` (
  `id` INT UNSIGNED NOT NULL AUTO_INCREMENT,
  `user_id` INT UNSIGNED NOT NULL,
  `account_id` INT UNSIGNED NOT NULL,
  `category_id` INT UNSIGNED NOT NULL,
  `amount` DECIMAL(10,2) NOT NULL,
  `expense_date` DATETIME NOT NULL,
  `description` TEXT NULL,
  `updated_at` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
  PRIMARY KEY (`id`),
  KEY `fk_expenses_user` (`user_id`),
  KEY `fk_expenses_account` (`account_id`),
  KEY `fk_expenses_category` (`category_id`),
  CONSTRAINT `fk_expenses_user` FOREIGN KEY (`user_id`) REFERENCES `Users` (`id`) ON DELETE CASCADE ON UPDATE CASCADE,
  CONSTRAINT `fk_expenses_account` FOREIGN KEY (`account_id`) REFERENCES `Accounts` (`id`) ON DELETE CASCADE ON UPDATE CASCADE,
  CONSTRAINT `fk_expenses_category` FOREIGN KEY (`category_id`) REFERENCES `ExpenseCategories` (`id`) ON DELETE RESTRICT ON UPDATE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- Incomes (income_date es DATE)
CREATE TABLE IF NOT EXISTS `Incomes` (
  `id` INT UNSIGNED NOT NULL AUTO_INCREMENT,
  `user_id` INT UNSIGNED NOT NULL,
  `account_id` INT UNSIGNED NOT NULL,
  `category_id` INT UNSIGNED NOT NULL,
  `amount` DECIMAL(10,2) NOT NULL,
  `income_date` DATE NOT NULL,
  `description` TEXT NULL,
  `created_at` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `updated_at` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
  PRIMARY KEY (`id`),
  KEY `idx_inc_user_date` (`user_id`,`income_date`),
  KEY `idx_inc_account_date` (`account_id`,`income_date`),
  KEY `idx_inc_category_date` (`category_id`,`income_date`),
  CONSTRAINT `fk_inc_user` FOREIGN KEY (`user_id`) REFERENCES `Users` (`id`) ON DELETE CASCADE ON UPDATE CASCADE,
  CONSTRAINT `fk_inc_account` FOREIGN KEY (`account_id`) REFERENCES `Accounts` (`id`) ON DELETE CASCADE ON UPDATE CASCADE,
  CONSTRAINT `fk_inc_cat` FOREIGN KEY (`category_id`) REFERENCES `IncomeCategories` (`id`) ON DELETE RESTRICT ON UPDATE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- Budgets
CREATE TABLE IF NOT EXISTS `Budgets` (
  `id` INT UNSIGNED NOT NULL AUTO_INCREMENT,
  `user_id` INT UNSIGNED NOT NULL,
  `category_id` INT UNSIGNED NOT NULL,
  `amount_limit` DECIMAL(10,2) NOT NULL,
  `period_start` DATE NOT NULL,
  `period_end` DATE NOT NULL,
  `notes` VARCHAR(255) NULL,
  `created_at` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `updated_at` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
  PRIMARY KEY (`id`),
  UNIQUE KEY `uq_budget_user_cat_period` (`user_id`,`category_id`,`period_start`,`period_end`),
  KEY `idx_budget_user_period` (`user_id`,`period_start`,`period_end`),
  KEY `idx_budget_category_period` (`category_id`,`period_start`,`period_end`),
  CONSTRAINT `fk_budget_user` FOREIGN KEY (`user_id`) REFERENCES `Users` (`id`) ON DELETE CASCADE ON UPDATE CASCADE,
  CONSTRAINT `fk_budget_category` FOREIGN KEY (`category_id`) REFERENCES `ExpenseCategories` (`id`) ON DELETE RESTRICT ON UPDATE CASCADE,
  CONSTRAINT `chk_budget_dates` CHECK ((`period_end` >= `period_start`))
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- Semillas de tipos de cuenta (idempotente por UNIQUE name)
INSERT IGNORE INTO `AccountTypes` (`name`) VALUES ('Efectivo'), ('Banco'), ('Ahorros');

-- Vista: gasto real agregado por mes y categoría
CREATE OR REPLACE VIEW `RealSpentMonthly` AS
SELECT
  e.`user_id`      AS `user_id`,
  e.`category_id`  AS `category_id`,
  YEAR(e.`expense_date`)  AS `y`,
  MONTH(e.`expense_date`) AS `m`,
  SUM(e.`amount`)  AS `spent_amount`
FROM `Expenses` e
GROUP BY e.`user_id`, e.`category_id`, YEAR(e.`expense_date`), MONTH(e.`expense_date`);
