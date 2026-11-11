-- =========================================================
-- Script0007_ExpenseSplitSpread
-- Gastos parciales (pagas solo una parte) y prorrateados (repartidos en meses).
-- Aditivo: solo ADD COLUMN nullable, no toca ni borra datos existentes.
-- Los gastos que ya hay quedan con todo a NULL = ni parcial ni prorrateado.
-- DbUp lo ejecuta una vez.
-- =========================================================

-- --- Gasto parcial: amount pasa a ser TU parte; aqui se guarda el contexto ---
ALTER TABLE `Expenses` ADD COLUMN `total_amount` DECIMAL(10,2) NULL;
ALTER TABLE `Expenses` ADD COLUMN `split_parts`  INT UNSIGNED  NULL;

-- --- Gasto prorrateado: N filas hermanas, una por mes ---
ALTER TABLE `Expenses` ADD COLUMN `spread_group` CHAR(36)     NULL;
ALTER TABLE `Expenses` ADD COLUMN `spread_index` INT UNSIGNED NULL;
ALTER TABLE `Expenses` ADD COLUMN `spread_count` INT UNSIGNED NULL;

CREATE INDEX `ix_expenses_spread_group` ON `Expenses` (`spread_group`);
