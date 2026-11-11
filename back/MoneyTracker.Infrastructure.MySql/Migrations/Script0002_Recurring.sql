-- =========================================================
-- Script0002_Recurring
-- Añade el flag "fijo/recurrente" a ingresos y gastos.
-- Aditivo y seguro (no borra datos). DbUp lo ejecuta una sola vez.
-- =========================================================

ALTER TABLE `Incomes`  ADD COLUMN `is_recurring` TINYINT(1) NOT NULL DEFAULT 0;
ALTER TABLE `Expenses` ADD COLUMN `is_recurring` TINYINT(1) NOT NULL DEFAULT 0;
