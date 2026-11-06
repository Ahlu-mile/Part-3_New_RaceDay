-- PROG6212 Part 3 schema upgrade. Safe additive change; does not drop or recreate tables.
-- Run against the existing RaceDay database used by RaceDay.API.
IF COL_LENGTH('dbo.Events', 'BannerImageUrl') IS NULL
    ALTER TABLE dbo.Events ADD BannerImageUrl nvarchar(1000) NULL;
GO
IF COL_LENGTH('dbo.Users', 'ProfileImageUrl') IS NULL
    ALTER TABLE dbo.Users ADD ProfileImageUrl nvarchar(1000) NULL;
GO
