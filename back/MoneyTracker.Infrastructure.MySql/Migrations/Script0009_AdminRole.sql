-- =========================================================
-- Script0009_AdminRole
-- Rol de administrador. El admin es quien decide que categorias son COMUNES
-- (user_id NULL, visibles para todos) y quienes son propias de un usuario.
-- Un usuario normal solo gestiona las suyas y ve las comunes en modo lectura.
--
-- Aditivo: ADD COLUMN con DEFAULT 0 (nadie es admin salvo quien se marque abajo).
-- DbUp lo ejecuta una sola vez.
-- =========================================================

ALTER TABLE `Users` ADD COLUMN `is_admin` TINYINT(1) NOT NULL DEFAULT 0;

-- El usuario de josema queda como admin (por email, que es unico en Users).
UPDATE `Users` SET `is_admin` = 1
WHERE `email` = 'admin@example.com';
