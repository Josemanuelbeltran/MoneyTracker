-- =========================================================
-- Script0013_PetUser
-- Las mascotas pasan a ser de quien las crea: cada usuario ve las suyas y solo
-- las suyas, igual que la agenda de People.
--
-- La clave pasa a ser (user_id, id) en vez de solo el id. El id sigue siendo el
-- de PetRecording, pero ahora es unico DENTRO de cada usuario: si fuese global,
-- el segundo en dar de alta "su mascota 1" chocaria con la de otro y recibiria
-- un error sobre una mascota que ni siquiera puede ver.
--
-- La tabla se creo en el Script0012 y a estas alturas deberia estar vacia; el
-- UPDATE es solo por si alguien metio filas a mano: se le asignan al admin.
-- =========================================================

ALTER TABLE `Pets` ADD COLUMN `user_id` INT UNSIGNED NULL;

UPDATE `Pets`
   SET `user_id` = COALESCE(
         (SELECT MIN(`id`) FROM `Users` WHERE `is_admin` = 1),
         (SELECT MIN(`id`) FROM `Users`))
 WHERE `user_id` IS NULL;

ALTER TABLE `Pets` MODIFY COLUMN `user_id` INT UNSIGNED NOT NULL;

ALTER TABLE `Pets` DROP PRIMARY KEY, ADD PRIMARY KEY (`user_id`, `id`);

ALTER TABLE `Pets`
  ADD CONSTRAINT `fk_pets_user` FOREIGN KEY (`user_id`)
  REFERENCES `Users` (`id`) ON DELETE CASCADE ON UPDATE CASCADE;
