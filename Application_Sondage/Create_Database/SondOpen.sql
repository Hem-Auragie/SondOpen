CREATE TABLE Sondage(
	IdSondage int IDENTITY(1,1) NOT NULL,
	ChoixMultiple bit NOT NULL,
	Question nvarchar(150) NOT NULL,
	NbVotes int NOT NULL,
	CleUnique INT  NOT NULL,
	PRIMARY KEY (IdSondage)
 );

CREATE TABLE Choix(
	IdChoix int IDENTITY(1,1) NOT NULL,
	IntituleChoix nvarchar(150) NOT NULL,
	FK_Id_Sondage int NOT NULL,
	NbVotes int NOT NULL,
	PRIMARY KEY (IdChoix),
	FOREIGN KEY (FK_Id_Sondage) REFERENCES Sondage(IdSondage)
 );
