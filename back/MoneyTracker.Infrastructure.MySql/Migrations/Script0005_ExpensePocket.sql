-- =========================================================
-- Script0005_ExpensePocket
-- Un gasto puede salir opcionalmente de un pocket concreto.
-- Aditivo (ADD COLUMN + FK con ON DELETE SET NULL): si se borra el pocket,
-- el gasto se conserva y su pocket_id pasa a NULL. DbUp lo ejecuta una vez.
-- =========================================================

ALTER TABLE `Expenses` ADD COLUMN `pocket_id` INT UNSIGNED NULL;

ALTER TABLE `Expenses`
  ADD CONSTRAINT `fk_exp_pocket` FOREIGN KEY (`pocket_id`)
  REFERENCES `Pockets` (`id`) ON DELETE SET NULL ON UPDATE CASCADE;
