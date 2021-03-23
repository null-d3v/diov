UPDATE [Content]
SET [PublishedDateTime] = '2000-01-01'
WHERE [PublishedDateTime] IS NULL;

ALTER TABLE [Content]
ALTER COLUMN [PublishedDateTime] DATETIMEOFFSET(7) NOT NULL;