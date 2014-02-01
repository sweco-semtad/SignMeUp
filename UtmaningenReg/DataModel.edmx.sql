
-- --------------------------------------------------
-- Entity Designer DDL Script for SQL Server 2005, 2008, and Azure
-- --------------------------------------------------
-- Date Created: 06/17/2013 22:27:03
-- Generated from EDMX file: C:\Projekt\Utmaningen2013_svn\trunk\UtmaningenReg\DataModel.edmx
-- --------------------------------------------------

SET QUOTED_IDENTIFIER OFF;
GO
USE [Utmaningen];
GO
IF SCHEMA_ID(N'dbo') IS NULL EXECUTE(N'CREATE SCHEMA [dbo]');
GO

-- --------------------------------------------------
-- Dropping existing FOREIGN KEY constraints
-- --------------------------------------------------

IF OBJECT_ID(N'[dbo].[FK_Registreringar_Banor]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[Registreringar] DROP CONSTRAINT [FK_Registreringar_Banor];
GO
IF OBJECT_ID(N'[dbo].[FK_Registreringar_Kanoter]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[Registreringar] DROP CONSTRAINT [FK_Registreringar_Kanoter];
GO
IF OBJECT_ID(N'[dbo].[FK_Registreringar_Klasser]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[Registreringar] DROP CONSTRAINT [FK_Registreringar_Klasser];
GO
IF OBJECT_ID(N'[dbo].[FK_InvoiceRegistreringar]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[InvoiceSet] DROP CONSTRAINT [FK_InvoiceRegistreringar];
GO
IF OBJECT_ID(N'[dbo].[FK_RegistreringarDeltagare]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[DeltagareSet] DROP CONSTRAINT [FK_RegistreringarDeltagare];
GO

-- --------------------------------------------------
-- Dropping existing tables
-- --------------------------------------------------

IF OBJECT_ID(N'[dbo].[Banor]', 'U') IS NOT NULL
    DROP TABLE [dbo].[Banor];
GO
IF OBJECT_ID(N'[dbo].[Kanoter]', 'U') IS NOT NULL
    DROP TABLE [dbo].[Kanoter];
GO
IF OBJECT_ID(N'[dbo].[Klasser]', 'U') IS NOT NULL
    DROP TABLE [dbo].[Klasser];
GO
IF OBJECT_ID(N'[dbo].[StartOchSlut]', 'U') IS NOT NULL
    DROP TABLE [dbo].[StartOchSlut];
GO
IF OBJECT_ID(N'[dbo].[Registreringar]', 'U') IS NOT NULL
    DROP TABLE [dbo].[Registreringar];
GO
IF OBJECT_ID(N'[dbo].[Forseningsavgift]', 'U') IS NOT NULL
    DROP TABLE [dbo].[Forseningsavgift];
GO
IF OBJECT_ID(N'[dbo].[InvoiceSet]', 'U') IS NOT NULL
    DROP TABLE [dbo].[InvoiceSet];
GO
IF OBJECT_ID(N'[dbo].[DeltagareSet]', 'U') IS NOT NULL
    DROP TABLE [dbo].[DeltagareSet];
GO
IF OBJECT_ID(N'[dbo].[Rabatter]', 'U') IS NOT NULL
    DROP TABLE [dbo].[Rabatter];
GO

-- --------------------------------------------------
-- Creating all tables
-- --------------------------------------------------

-- Creating table 'Banor'
CREATE TABLE [dbo].[Banor] (
    [ID] int IDENTITY(1,1) NOT NULL,
    [Namn] nvarchar(50)  NOT NULL,
    [Avgift] int  NOT NULL,
    [AntalDeltagare] int  NOT NULL
);
GO

-- Creating table 'Kanoter'
CREATE TABLE [dbo].[Kanoter] (
    [ID] int IDENTITY(1,1) NOT NULL,
    [Namn] nvarchar(50)  NULL,
    [Avgift] int  NULL
);
GO

-- Creating table 'Klasser'
CREATE TABLE [dbo].[Klasser] (
    [ID] int IDENTITY(1,1) NOT NULL,
    [Namn] nvarchar(max)  NULL
);
GO

-- Creating table 'StartOchSlut'
CREATE TABLE [dbo].[StartOchSlut] (
    [ID] int  NOT NULL,
    [Namn] nvarchar(50)  NOT NULL,
    [Datum] datetime  NOT NULL
);
GO

-- Creating table 'Registreringar'
CREATE TABLE [dbo].[Registreringar] (
    [ID] int IDENTITY(1,1) NOT NULL,
    [Adress] nvarchar(max)  NOT NULL,
    [Telefon] nvarchar(50)  NOT NULL,
    [Epost] nvarchar(max)  NOT NULL,
    [Ranking] bit  NOT NULL,
    [Startnummer] int  NOT NULL,
    [Lagnamn] nvarchar(max)  NOT NULL,
    [Kanot] int  NOT NULL,
    [Klubb] nvarchar(max)  NULL,
    [Klass] int  NOT NULL,
    [HarBetalt] bit  NOT NULL,
    [Forseningsavgift] int  NOT NULL,
    [Registreringstid] datetime  NOT NULL,
    [Kommentar] nvarchar(max)  NULL,
    [Bana] int  NOT NULL,
    [Rabatt] int  NOT NULL,
    [PaysonToken] nvarchar(max)  NULL
);
GO

-- Creating table 'Forseningsavgift'
CREATE TABLE [dbo].[Forseningsavgift] (
    [ID] int IDENTITY(1,1) NOT NULL,
    [FranDatum] datetime  NOT NULL,
    [Summa] int  NOT NULL
);
GO

-- Creating table 'InvoiceSet'
CREATE TABLE [dbo].[InvoiceSet] (
    [Id] int IDENTITY(1,1) NOT NULL,
    [Box] nvarchar(max)  NULL,
    [Postnummer] nvarchar(max)  NOT NULL,
    [Organisationsnummer] nvarchar(max)  NOT NULL,
    [Postort] nvarchar(max)  NOT NULL,
    [Postadress] nvarchar(max)  NOT NULL,
    [Namn] nvarchar(max)  NOT NULL,
    [Att] nvarchar(max)  NULL,
    [Registreringar_ID] int  NOT NULL
);
GO

-- Creating table 'DeltagareSet'
CREATE TABLE [dbo].[DeltagareSet] (
    [Id] int IDENTITY(1,1) NOT NULL,
    [Förnamn] nvarchar(max)  NOT NULL,
    [Efternamn] nvarchar(max)  NOT NULL,
    [Personnummer] nvarchar(max)  NULL,
    [RegistreringarID] int  NOT NULL
);
GO

-- Creating table 'Rabatter'
CREATE TABLE [dbo].[Rabatter] (
    [Id] int IDENTITY(1,1) NOT NULL,
    [Kod] nvarchar(max)  NOT NULL,
    [Summa] int  NOT NULL,
    [Beskrivning] nvarchar(max)  NOT NULL
);
GO

-- --------------------------------------------------
-- Creating all PRIMARY KEY constraints
-- --------------------------------------------------

-- Creating primary key on [ID] in table 'Banor'
ALTER TABLE [dbo].[Banor]
ADD CONSTRAINT [PK_Banor]
    PRIMARY KEY CLUSTERED ([ID] ASC);
GO

-- Creating primary key on [ID] in table 'Kanoter'
ALTER TABLE [dbo].[Kanoter]
ADD CONSTRAINT [PK_Kanoter]
    PRIMARY KEY CLUSTERED ([ID] ASC);
GO

-- Creating primary key on [ID] in table 'Klasser'
ALTER TABLE [dbo].[Klasser]
ADD CONSTRAINT [PK_Klasser]
    PRIMARY KEY CLUSTERED ([ID] ASC);
GO

-- Creating primary key on [ID] in table 'StartOchSlut'
ALTER TABLE [dbo].[StartOchSlut]
ADD CONSTRAINT [PK_StartOchSlut]
    PRIMARY KEY CLUSTERED ([ID] ASC);
GO

-- Creating primary key on [ID] in table 'Registreringar'
ALTER TABLE [dbo].[Registreringar]
ADD CONSTRAINT [PK_Registreringar]
    PRIMARY KEY CLUSTERED ([ID] ASC);
GO

-- Creating primary key on [ID] in table 'Forseningsavgift'
ALTER TABLE [dbo].[Forseningsavgift]
ADD CONSTRAINT [PK_Forseningsavgift]
    PRIMARY KEY CLUSTERED ([ID] ASC);
GO

-- Creating primary key on [Id] in table 'InvoiceSet'
ALTER TABLE [dbo].[InvoiceSet]
ADD CONSTRAINT [PK_InvoiceSet]
    PRIMARY KEY CLUSTERED ([Id] ASC);
GO

-- Creating primary key on [Id] in table 'DeltagareSet'
ALTER TABLE [dbo].[DeltagareSet]
ADD CONSTRAINT [PK_DeltagareSet]
    PRIMARY KEY CLUSTERED ([Id] ASC);
GO

-- Creating primary key on [Id] in table 'Rabatter'
ALTER TABLE [dbo].[Rabatter]
ADD CONSTRAINT [PK_Rabatter]
    PRIMARY KEY CLUSTERED ([Id] ASC);
GO

-- --------------------------------------------------
-- Creating all FOREIGN KEY constraints
-- --------------------------------------------------

-- Creating foreign key on [Bana] in table 'Registreringar'
ALTER TABLE [dbo].[Registreringar]
ADD CONSTRAINT [FK_Registreringar_Banor]
    FOREIGN KEY ([Bana])
    REFERENCES [dbo].[Banor]
        ([ID])
    ON DELETE NO ACTION ON UPDATE NO ACTION;

-- Creating non-clustered index for FOREIGN KEY 'FK_Registreringar_Banor'
CREATE INDEX [IX_FK_Registreringar_Banor]
ON [dbo].[Registreringar]
    ([Bana]);
GO

-- Creating foreign key on [Kanot] in table 'Registreringar'
ALTER TABLE [dbo].[Registreringar]
ADD CONSTRAINT [FK_Registreringar_Kanoter]
    FOREIGN KEY ([Kanot])
    REFERENCES [dbo].[Kanoter]
        ([ID])
    ON DELETE NO ACTION ON UPDATE NO ACTION;

-- Creating non-clustered index for FOREIGN KEY 'FK_Registreringar_Kanoter'
CREATE INDEX [IX_FK_Registreringar_Kanoter]
ON [dbo].[Registreringar]
    ([Kanot]);
GO

-- Creating foreign key on [Klass] in table 'Registreringar'
ALTER TABLE [dbo].[Registreringar]
ADD CONSTRAINT [FK_Registreringar_Klasser]
    FOREIGN KEY ([Klass])
    REFERENCES [dbo].[Klasser]
        ([ID])
    ON DELETE NO ACTION ON UPDATE NO ACTION;

-- Creating non-clustered index for FOREIGN KEY 'FK_Registreringar_Klasser'
CREATE INDEX [IX_FK_Registreringar_Klasser]
ON [dbo].[Registreringar]
    ([Klass]);
GO

-- Creating foreign key on [Registreringar_ID] in table 'InvoiceSet'
ALTER TABLE [dbo].[InvoiceSet]
ADD CONSTRAINT [FK_InvoiceRegistreringar]
    FOREIGN KEY ([Registreringar_ID])
    REFERENCES [dbo].[Registreringar]
        ([ID])
    ON DELETE CASCADE ON UPDATE NO ACTION;

-- Creating non-clustered index for FOREIGN KEY 'FK_InvoiceRegistreringar'
CREATE INDEX [IX_FK_InvoiceRegistreringar]
ON [dbo].[InvoiceSet]
    ([Registreringar_ID]);
GO

-- Creating foreign key on [RegistreringarID] in table 'DeltagareSet'
ALTER TABLE [dbo].[DeltagareSet]
ADD CONSTRAINT [FK_RegistreringarDeltagare]
    FOREIGN KEY ([RegistreringarID])
    REFERENCES [dbo].[Registreringar]
        ([ID])
    ON DELETE NO ACTION ON UPDATE NO ACTION;

-- Creating non-clustered index for FOREIGN KEY 'FK_RegistreringarDeltagare'
CREATE INDEX [IX_FK_RegistreringarDeltagare]
ON [dbo].[DeltagareSet]
    ([RegistreringarID]);
GO

-- --------------------------------------------------
-- Script has ended
-- --------------------------------------------------