ALTER TABLE `Payroll` 
ADD COLUMN `IsContractor` TINYINT NOT NULL DEFAULT '0' AFTER `BaseSalaryCents`;
