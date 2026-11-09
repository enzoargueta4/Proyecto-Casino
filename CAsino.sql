--CREAR BD

CREATE DATABASE DB_AZAR_USAM;
GO
USE DB_AZAR_USAM;
GO

-- TABLA 1: Jugadores
CREATE TABLE Jugador (
    Id_Jugador INT PRIMARY KEY IDENTITY(1,1),
    Nombre VARCHAR(100) NOT NULL,
    Carnet VARCHAR(10) NOT NULL,
    Saldo DECIMAL(10,2) DEFAULT 1000.00,
    FechaRegistro DATETIME DEFAULT GETDATE()
);

-- TABLA 2: Historial de Partidas
CREATE TABLE Partida (
    Id_Partida INT PRIMARY KEY IDENTITY(1,1),
    Id_Jugador INT NOT NULL,
    Fecha DATETIME DEFAULT GETDATE(),
    TipoApuesta VARCHAR(20), -- ROJO, NEGRO, NUMERO
    Eleccion VARCHAR(20),
    Resultado INT,
    MontoApostado DECIMAL(10,2),
    Ganancia DECIMAL(10,2),
    SaldoFinal DECIMAL(10,2),
    FOREIGN KEY (Id_Jugador) REFERENCES Jugador(Id_Jugador)
);

INSERT INTO Jugador (Nombre, Carnet, Saldo) VALUES
('Enzo Fabricio Argueta Alas', '261088', 1000),
('Jose Francisco Rivera Vasquez', '260346', 1000),
('Carlos Edenilson Osorio Magaña', '260218', 1000);

-- Consulta 1: Ver todo el historial con nombres (para tu DataGridView)
SELECT 
    p.Fecha,
    j.Nombre,
    j.Carnet,
    p.TipoApuesta,
    p.Eleccion,
    p.Resultado,
    p.MontoApostado,
    p.Ganancia
FROM Partida p
INNER JOIN Jugador j ON p.Id_Jugador = j.Id_Jugador
ORDER BY p.Fecha DESC;

-- Consulta 2: Ranking de ganadores (quien tiene más saldo)
SELECT Nombre, Carnet, Saldo 
FROM Jugador 
ORDER BY Saldo DESC;

-- Consulta 3: Total ganado/perdido por jugador
SELECT 
    j.Nombre,
    SUM(p.Ganancia) as Total_Ganancias,
    COUNT(p.Id_Partida) as Total_Partidas
FROM Jugador j
LEFT JOIN Partida p ON j.Id_Jugador = p.Id_Jugador
GROUP BY j.Nombre;