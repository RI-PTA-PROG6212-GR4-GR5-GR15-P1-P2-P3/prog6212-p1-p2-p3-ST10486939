-- CREATE DATABASE 
CREATE DATABASE RaceDay; 

--CREATE TABLES
CREATE TABLE UserRole(
	roleID	INT IDENTITY(1,1) PRIMARY KEY,
	description VARCHAR(50)
);

CREATE TABLE EventOrganizer(
	organizerID INT IDENTITY(1,1) PRIMARY KEY,
	name VARCHAR(100) NOT NULL,
	roleID INT NOT NULL,
	emailAddress VARCHAR(100) NOT NULL,
	password VARCHAR(50) NOT NULL,

	CONSTRAINT FK_EventOrganizer_roleID
	FOREIGN KEY (roleID)
	REFERENCES UserRole(roleID)
);

CREATE TABLE Participant(
	participantID INT IDENTITY (100,1) PRIMARY KEY,
	name VARCHAR(100) NOT NULL,
	age INT NOT NULL,
	roleID INT NOT NULL,
	emailAddress VARCHAR(100) NOT NULL,

	CONSTRAINT FK_Participant_roleID
	FOREIGN KEY (roleID)
	REFERENCES UserRole(roleID)
);

CREATE TABLE Category(
	categoryID INT IDENTITY(1,1) PRIMARY KEY,
	description VARCHAR(50) NOT NULL
);

CREATE TABLE Event(
	eventID INT IDENTITY(300,1) PRIMARY KEY,
	organizerID INT NOT NULL,
	eventName VARCHAR(100) NOT NULL,
	eventLocation VARCHAR(50) NOT NULL,
	description VARCHAR(100) NOT NULL,
	distance VARCHAR(50) NOT NULL,
	categoryID INT NOT NULL, 
	type VARCHAR(100) NOT NULL,

	CONSTRAINT FK_Event_organizerID
	FOREIGN KEY (organizerID)
	REFERENCES EventOrganizer(organizerID),

	CONSTRAINT FK_EVENT_categoryID
	FOREIGN KEY (categoryID)
	REFERENCES Category(categoryID)
);

CREATE TABLE Entries(
	entryID INT IDENTITY(1,1) PRIMARY KEY,
	eventID INT NOT NULL,
	participantID INT NOT NULL

	CONSTRAINT FK_Entries_eventID
	FOREIGN KEY (eventID)
	REFERENCES Event(eventID),

	CONSTRAINT FK_Entries_participantID
	FOREIGN KEY (participantID)
	REFERENCES Participant(participantID)
);

CREATE TABLE ParticipantResults(
	participantID INT NOT NULL,
	entryID INT NOT NULL,
	completionTime VARCHAR(50),

	CONSTRAINT FK_ParticipantResults_participantID
	FOREIGN KEY (participantID)
	REFERENCES Participant(participantID),

	CONSTRAINT FK_ParticipantResults_entryID
	FOREIGN KEY (entryID)
	REFERENCES Entries(entryID)
);

--ALTERING Event TABLE TO ADD A COLUMN FOR THE DATE
ALTER TABLE Event ADD eventDate DATE NOT NULL;

--POPULATING THE DATABASE 
INSERT INTO UserRole 
VALUES	('Organizer'),
		('Participant');

INSERT INTO EventOrganizer 
VALUES	('Pretoria Marathon Club', 1, 'PTAMarathonclub&email.com', 'Marathoner858'),
		('In Reach Running', 1, 'InReach443@outlook.com', 'RunningHigh6767'),
		('Hartenbos Drawwers', 1, 'Hartenbos89!@gmail.com', 'H@rtenb0$$');

INSERT INTO Participant
VALUES	('Nonhlanhla Mahlangu', 35, 2, 'nmahlangu182@outlook.com'),
		('Haley Lenae', 21, 2, 'HLenae1!@gmail.com'), 
		('Gina Lennetti', 27, 2, 'GinaL1nnett!Confett!@gmail.com'),
		('Nkanyiso Ntuli',24, 2, 'NNtuli98974@gmail.com'),
		('Sandra Johnson', 18, 2, 'SandraAintWorried182@hotmail.com');

INSERT INTO Category
VALUES	('Running'),
		('Cycling'),
		('Walking');

INSERT INTO Event
VALUES
(3, 'Mossel Bay Sportfees', 'Hartenbos', '15km and 4km road race', '15KM & 4KM', 1, '15KM road race & 4KM run/walk', 
'2026-10-03'),
(1, 'Brooklyn Road Race', 'Pretoria', '33km, 21km, 10km, 5km races', '33KM & 21KM & 10KM, 5KM', 1, '33KM race, 22km 
(half marathon), 10km race and 5km fun run', '2026-09-05'),
(2, 'The Jackal Night Run & Walk', 'Roodepoort', '10km and 5km race', '10KM & 5KM', 1, '10KM race and 5KM run/walk', 
'2026-09-04'),
(1, 'PwC George Claassen Memorial Race', 'Pretoria', '21km, 10km, and 5 km races', '21KM & 10KM & 5KM', 1, 'Half 
marathon (21km), 10km run/walk, 5km run/walk', '2027-01-16');

INSERT INTO Entries
VALUES	(300, 101), 
		(301, 100),
		(302, 103),
		(303, 102),
		(301, 104);