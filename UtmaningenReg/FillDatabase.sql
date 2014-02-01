USE [Utmaningen];
GO

INSERT INTO Banor
VALUES ('Svenska Cupen', 800, 2)

INSERT INTO Banor
VALUES ('Utmaningen', 600, 2)

INSERT INTO Banor
VALUES ('Utmaningen Stafett (kanot ingår)', 1950, 4)


INSERT INTO Kanoter
VALUES ('Ingen', 0)

INSERT INTO Kanoter
VALUES ('Stafett, kanot ingår', 0)

INSERT INTO Kanoter
VALUES ('Kanadensare', 350)

INSERT INTO Kanoter
VALUES ('Acron', 400)


INSERT INTO Klasser
VALUES ('Dam')

INSERT INTO Klasser
VALUES ('Herr')

INSERT INTO Klasser
VALUES ('Mix')

INSERT INTO Klasser
VALUES ('Stafett')


INSERT INTO StartOchSlut
VALUES (1, 'Start', '2013-01-01')

INSERT INTO StartOchSlut
VALUES (2, 'Slut', '2013-08-19')


INSERT INTO Forseningsavgift
VALUES ('2013-07-15', 100)

INSERT INTO Forseningsavgift
VALUES ('2013-08-10', 200)