ALTER TABLE Registreringar
ADD PaysonToken nvarchar(MAX)

ALTER TABLE Banor ALTER COLUMN Avgift int not null;
ALTER TABLE Banor ALTER COLUMN Namn nvarchar(50) not null;

-- Creating table 'Rabatter'
CREATE TABLE [dbo].[Rabatter] (
    [Id] int IDENTITY(1,1) NOT NULL,
    [Kod] nvarchar(max)  NOT NULL,
    [Summa] int  NOT NULL,
    [Beskrivning] nvarchar(max)  NOT NULL
);
GO

-- Creating primary key on [Id] in table 'Rabatter'
ALTER TABLE [dbo].[Rabatter]
ADD CONSTRAINT [PK_Rabatter]
    PRIMARY KEY CLUSTERED ([Id] ASC);
GO