-- =========================================================
-- Script0012_ExpensePet
-- Gastos de mascota: los que van a la categoria Mascotas se mandan a
-- PetRecording (otra app, en la misma maquina) al crearlos, editarlos y
-- borrarlos.
--
-- `Pets` es solo una tabla auxiliar para el desplegable: el id NO es
-- autoincremental porque tiene que ser EL MISMO que en PetRecording, asi que
-- se escribe a mano. El nombre es para elegir la mascota en el formulario.
--
-- Tampoco hay FK de Expenses -> Pets a proposito: el id que manda es el de
-- PetRecording, y esta tabla es un espejo para poder enseñar el nombre. Una FK
-- obligaria a dar de alta aqui una mascota antes de poder apuntarle un gasto.
--
-- Aditivo: tabla nueva + una columna nullable. No toca datos existentes.
-- =========================================================

CREATE TABLE IF NOT EXISTS `Pets` (
  `id`   INT UNSIGNED NOT NULL,          -- el mismo id que en PetRecording
  `name` VARCHAR(100) NOT NULL,
  PRIMARY KEY (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- A que mascota fue el gasto. NULL = no es un gasto de mascota.
ALTER TABLE `Expenses` ADD COLUMN `pet_id` INT UNSIGNED NULL;
CREATE INDEX `ix_expenses_pet` ON `Expenses` (`pet_id`);
