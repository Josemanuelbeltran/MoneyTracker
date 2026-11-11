-- =========================================================
-- Script0010_ExpenseReimbursements
-- Gasto compartido con reparto libre (70/30) donde TU adelantas el total y
-- te devuelven su parte (tipicamente por bizum) a una cuenta concreta.
--
-- Modelo: el `amount` del gasto sigue siendo TU parte (lo que de verdad te
-- cuesta, lo que suman estadisticas y presupuestos). El dinero que adelantas
-- y te tienen que devolver vive aqui:
--   settled_at NULL      -> pendiente de cobro (el dinero ya salio de tu cuenta)
--   settled_at con fecha -> cobrado (entro en `to_account_id`)
--
-- El saldo de la cuenta con la que pagaste resta el reembolso desde el primer
-- momento (el dinero salio); la cuenta de destino lo suma solo al cobrarlo.
--
-- Aditivo: solo crea una tabla nueva. No toca ni una fila existente.
-- DbUp lo ejecuta una sola vez.
-- =========================================================

CREATE TABLE IF NOT EXISTS `ExpenseReimbursements` (
  `id`            INT UNSIGNED   NOT NULL AUTO_INCREMENT,
  `user_id`       INT UNSIGNED   NOT NULL,
  -- Gasto que se adelanto. Si se borra el gasto, el reembolso se va con el.
  `expense_id`    INT UNSIGNED   NOT NULL,
  -- Lo que te tienen que devolver = total del gasto - tu parte.
  `amount`        DECIMAL(10,2)  NOT NULL,
  -- Cuenta donde te haran el bizum (o donde metes el efectivo que te den).
  `to_account_id` INT UNSIGNED   NOT NULL,
  -- NULL = pendiente. Con fecha = ya cobrado.
  `settled_at`    DATETIME       NULL,
  -- Quien te lo debe, opcional y libre ("Ana", "los del piso"...).
  `debtor`        VARCHAR(100)   NULL,
  `created_at`    DATETIME       NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `updated_at`    DATETIME       NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
  PRIMARY KEY (`id`),
  KEY `ix_expreimb_user` (`user_id`),
  KEY `ix_expreimb_expense` (`expense_id`),
  -- Para sacar rapido "lo que me deben" de un usuario.
  KEY `ix_expreimb_pending` (`user_id`, `settled_at`),
  CONSTRAINT `fk_expreimb_user` FOREIGN KEY (`user_id`)
    REFERENCES `Users` (`id`) ON DELETE CASCADE ON UPDATE CASCADE,
  CONSTRAINT `fk_expreimb_expense` FOREIGN KEY (`expense_id`)
    REFERENCES `Expenses` (`id`) ON DELETE CASCADE ON UPDATE CASCADE,
  -- RESTRICT: no dejamos borrar una cuenta que todavia espera un cobro.
  CONSTRAINT `fk_expreimb_account` FOREIGN KEY (`to_account_id`)
    REFERENCES `Accounts` (`id`) ON DELETE RESTRICT ON UPDATE CASCADE,
  CONSTRAINT `chk_expreimb_amount` CHECK ((`amount` > 0))
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;
