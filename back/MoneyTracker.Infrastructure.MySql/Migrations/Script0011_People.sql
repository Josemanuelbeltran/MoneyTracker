-- =========================================================
-- Script0011_People
-- Personas con las que compartes gastos (quien te debe el bizum). Son tuyas:
-- cada usuario tiene su propia agenda, no se comparten entre usuarios.
--
-- El reembolso guarda las dos cosas:
--   person_id -> enlace a la persona, para poder agrupar y reutilizarla
--   debtor    -> el nombre tal cual estaba al crearlo, asi que si borras a la
--                persona el historico sigue diciendo quien era
--
-- Aditivo: tabla nueva + una columna nullable. DbUp lo ejecuta una sola vez.
-- =========================================================

CREATE TABLE IF NOT EXISTS `People` (
  `id`         INT UNSIGNED NOT NULL AUTO_INCREMENT,
  `user_id`    INT UNSIGNED NOT NULL,
  `name`       VARCHAR(100) NOT NULL,
  `created_at` DATETIME     NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `updated_at` DATETIME     NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
  PRIMARY KEY (`id`),
  -- No tiene sentido tener dos "Ana" en la agenda del mismo usuario.
  UNIQUE KEY `uq_people_user_name` (`user_id`, `name`),
  CONSTRAINT `fk_people_user` FOREIGN KEY (`user_id`)
    REFERENCES `Users` (`id`) ON DELETE CASCADE ON UPDATE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- Quien te debe el reembolso. NULL = no lo dijiste o la persona ya no existe.
ALTER TABLE `ExpenseReimbursements` ADD COLUMN `person_id` INT UNSIGNED NULL;
ALTER TABLE `ExpenseReimbursements`
  ADD CONSTRAINT `fk_expreimb_person` FOREIGN KEY (`person_id`)
  REFERENCES `People` (`id`) ON DELETE SET NULL ON UPDATE CASCADE;
CREATE INDEX `ix_expreimb_person` ON `ExpenseReimbursements` (`person_id`);
