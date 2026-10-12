
USE ParcialProgramacion1;

CREATE TABLE dbo.Productos
(
IdProducto INT IDENTITY(1,1) PRIMARY KEY,
Nombre NVARCHAR(100) NOT NULL,
Categoria NVARCHAR(50) NOT NULL,
Precio DECIMAL(10,2) NOT NULL,
Existencias INT NOT NULL
);

SELECT * FROM Productos