namespace MoneyTracker.Infrastructure.MySql.Scripts;

public class BBDD
{
/*
    -- =========================================================
-- Reset (eliminar vista y tablas si existen)
-- =========================================================
DROP VIEW IF EXISTS `RealSpentMonthly`;

DROP TABLE IF EXISTS `Incomes`;
DROP TABLE IF EXISTS `Expenses`;
DROP TABLE IF EXISTS `Budgets`;
DROP TABLE IF EXISTS `Accounts`;
DROP TABLE IF EXISTS `AccountTypes`;
DROP TABLE IF EXISTS `ExpenseCategories`;
DROP TABLE IF EXISTS `IncomeCategories`;
DROP TABLE IF EXISTS `Users`;

-- =========================================================
-- Tabla: Users
-- =========================================================
CREATE TABLE `Users` (
  `id` INT UNSIGNED AUTO_INCREMENT PRIMARY KEY,
  `name` VARCHAR(50) NOT NULL,
  `password_hash` VARCHAR(255) NOT NULL,
  `phone_number` VARCHAR(15) NULL,
  `verified_phone` TINYINT(1) NOT NULL DEFAULT 0,
  `created_at` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `updated_at` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

-- =========================================================
-- Tabla: AccountTypes (tipos de cuenta)
-- =========================================================
CREATE TABLE `AccountTypes` (
  `id` INT UNSIGNED AUTO_INCREMENT PRIMARY KEY,
  `name` VARCHAR(50) NOT NULL UNIQUE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

-- (Opcional) Semillas básicas
INSERT INTO `AccountTypes` (`name`) VALUES ('Efectivo'), ('Banco'), ('Ahorros');

-- =========================================================
-- Tabla: Accounts (cuentas por usuario)
-- =========================================================
CREATE TABLE `Accounts` (
  `id` INT UNSIGNED AUTO_INCREMENT PRIMARY KEY,
  `user_id` INT UNSIGNED NOT NULL,
  `account_type_id` INT UNSIGNED NOT NULL,
  `name` VARCHAR(100) NOT NULL,
  `balance` DECIMAL(10,2) NOT NULL DEFAULT 0.00,
  `created_at` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `updated_at` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
  CONSTRAINT `fk_accounts_user` FOREIGN KEY (`user_id`) REFERENCES `Users`(`id`)
    ON DELETE CASCADE ON UPDATE CASCADE,
  CONSTRAINT `fk_accounts_type` FOREIGN KEY (`account_type_id`) REFERENCES `AccountTypes`(`id`)
    ON DELETE RESTRICT ON UPDATE CASCADE,
  INDEX `idx_accounts_user` (`user_id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

-- =========================================================
-- Tabla: ExpenseCategories (jerarquía categorías gastos)
-- =========================================================
CREATE TABLE `ExpenseCategories` (
  `id` INT UNSIGNED AUTO_INCREMENT PRIMARY KEY,
  `name` VARCHAR(100) NOT NULL,
  `parent_id` INT UNSIGNED NULL,
  `created_at` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `updated_at` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
  CONSTRAINT `fk_exp_cat_parent` FOREIGN KEY (`parent_id`) REFERENCES `ExpenseCategories`(`id`)
    ON DELETE CASCADE ON UPDATE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

-- =========================================================
-- Tabla: IncomeCategories (jerarquía categorías ingresos)
-- =========================================================
CREATE TABLE `IncomeCategories` (
  `id` INT UNSIGNED AUTO_INCREMENT PRIMARY KEY,
  `name` VARCHAR(100`) NOT NULL,
  `parent_id` INT UNSIGNED NULL,
  `created_at` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `updated_at` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
  CONSTRAINT `fk_inc_cat_parent` FOREIGN KEY (`parent_id`) REFERENCES `IncomeCategories`(`id`)
    ON DELETE CASCADE ON UPDATE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

-- =========================================================
-- Tabla: Expenses (gastos)
-- =========================================================
CREATE TABLE `Expenses` (
  `id` INT UNSIGNED AUTO_INCREMENT PRIMARY KEY,
  `user_id` INT UNSIGNED NOT NULL,
  `account_id` INT UNSIGNED NOT NULL,
  `category_id` INT UNSIGNED NOT NULL,      -- subcategoría de gasto
  `amount` DECIMAL(10,2) NOT NULL,
  `expense_date` DATE NOT NULL,
  `description` TEXT NULL,
  `created_at` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `updated_at` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
  CONSTRAINT `fk_exp_user`    FOREIGN KEY (`user_id`)    REFERENCES `Users`(`id`)
    ON DELETE CASCADE ON UPDATE CASCADE,
  CONSTRAINT `fk_exp_account` FOREIGN KEY (`account_id`) REFERENCES `Accounts`(`id`)
    ON DELETE CASCADE ON UPDATE CASCADE,
  CONSTRAINT `fk_exp_cat`     FOREIGN KEY (`category_id`) REFERENCES `ExpenseCategories`(`id`)
    ON DELETE RESTRICT ON UPDATE CASCADE,
  INDEX `idx_exp_user_date` (`user_id`, `expense_date`),
  INDEX `idx_exp_account_date` (`account_id`, `expense_date`),
  INDEX `idx_exp_category_date` (`category_id`, `expense_date`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

-- =========================================================
-- Tabla: Incomes (ingresos)
-- =========================================================
CREATE TABLE `Incomes` (
  `id` INT UNSIGNED AUTO_INCREMENT PRIMARY KEY,
  `user_id` INT UNSIGNED NOT NULL,
  `account_id` INT UNSIGNED NOT NULL,
  `category_id` INT UNSIGNED NOT NULL,      -- subcategoría de ingreso
  `amount` DECIMAL(10,2) NOT NULL,
  `income_date` DATE NOT NULL,
  `description` TEXT NULL,
  `created_at` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `updated_at` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
  CONSTRAINT `fk_inc_user`    FOREIGN KEY (`user_id`)    REFERENCES `Users`(`id`)
    ON DELETE CASCADE ON UPDATE CASCADE,
  CONSTRAINT `fk_inc_account` FOREIGN KEY (`account_id`) REFERENCES `Accounts`(`id`)
    ON DELETE CASCADE ON UPDATE CASCADE,
  CONSTRAINT `fk_inc_cat`     FOREIGN KEY (`category_id`) REFERENCES `IncomeCategories`(`id`)
    ON DELETE RESTRICT ON UPDATE CASCADE,
  INDEX `idx_inc_user_date` (`user_id`, `income_date`),
  INDEX `idx_inc_account_date` (`account_id`, `income_date`),
  INDEX `idx_inc_category_date` (`category_id`, `income_date`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

-- =========================================================
-- Tabla: Budgets (presupuestos por subcategoría de gasto y periodo)
-- =========================================================
CREATE TABLE `Budgets` (
  `id` INT UNSIGNED AUTO_INCREMENT PRIMARY KEY,
  `user_id` INT UNSIGNED NOT NULL,
  `category_id` INT UNSIGNED NOT NULL,          -- subcategoría de gasto
  `amount_limit` DECIMAL(10,2) NOT NULL,
  `period_start` DATE NOT NULL,
  `period_end`   DATE NOT NULL,
  `notes` VARCHAR(255) NULL,
  `created_at` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `updated_at` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
  CONSTRAINT `fk_budget_user`     FOREIGN KEY (`user_id`)    REFERENCES `Users`(`id`)
    ON DELETE CASCADE ON UPDATE CASCADE,
  CONSTRAINT `fk_budget_category` FOREIGN KEY (`category_id`) REFERENCES `ExpenseCategories`(`id`)
    ON DELETE RESTRICT ON UPDATE CASCADE,
  CONSTRAINT `chk_budget_dates` CHECK (`period_end` >= `period_start`),
  UNIQUE KEY `uq_budget_user_cat_period` (`user_id`, `category_id`, `period_start`, `period_end`),
  INDEX `idx_budget_user_period` (`user_id`, `period_start`, `period_end`),
  INDEX `idx_budget_category_period` (`category_id`, `period_start`, `period_end`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

-- =========================================================
-- Vista: RealSpentMonthly (gasto real agregado mensual)
-- =========================================================
CREATE OR REPLACE VIEW `RealSpentMonthly` AS
SELECT
  e.`user_id`,
  e.`category_id`,                -- subcategoría de gasto
  YEAR(e.`expense_date`) AS `y`,
  MONTH(e.`expense_date`) AS `m`,
  SUM(e.`amount`) AS `spent_amount`
FROM `Expenses` e
GROUP BY e.`user_id`, e.`category_id`, YEAR(e.`expense_date`), MONTH(e.`expense_date`);
*/
}