/* =====================================================================
   RaceDay - Full-Stack Event Management System
   Part 1 - Database Creation Script
   Student: ST10488271
   Module: PROG6212

   Run this script in SQL Server Management Studio (SSMS) against a
   clean SQL Server instance. It will:
     1. Create the RaceDayDB database
     2. Create all six (6) entity tables with PK/FK constraints
     3. Seed the database with realistic sample data
   ===================================================================== */

IF DB_ID('RaceDayDB') IS NOT NULL
BEGIN
    ALTER DATABASE RaceDayDB SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
    DROP DATABASE RaceDayDB;
END
GO

CREATE DATABASE RaceDayDB;
GO

USE RaceDayDB;
GO

/* =====================================================================
   1. USERS
   Stores both Organisers and Participants, differentiated by Role.
   ===================================================================== */
CREATE TABLE Users (
    UserID          INT IDENTITY(1,1)   NOT NULL,
    FullName        NVARCHAR(100)       NOT NULL,
    Email           NVARCHAR(150)       NOT NULL,
    PasswordHash    NVARCHAR(255)       NOT NULL,
    Role            VARCHAR(20)         NOT NULL
                        CONSTRAINT CK_Users_Role CHECK (Role IN ('Organiser','Participant')),
    PhoneNumber     VARCHAR(20)         NULL,
    CreatedAt       DATETIME            NOT NULL CONSTRAINT DF_Users_CreatedAt DEFAULT (GETDATE()),
    CONSTRAINT PK_Users PRIMARY KEY (UserID),
    CONSTRAINT UQ_Users_Email UNIQUE (Email)
);
GO

/* =====================================================================
   2. EVENTS
   Created by an Organiser (Users). One Organiser -> many Events.
   ===================================================================== */
CREATE TABLE Events (
    EventID         INT IDENTITY(1,1)   NOT NULL,
    OrganiserID     INT                 NOT NULL,
    EventName       NVARCHAR(150)       NOT NULL,
    Description     NVARCHAR(1000)      NULL,
    EventType       VARCHAR(20)         NOT NULL
                        CONSTRAINT CK_Events_Type CHECK (EventType IN ('Run','Walk','Cycle')),
    EventDate       DATE                NOT NULL,
    Location        NVARCHAR(150)       NOT NULL,
    StartTime       TIME                NOT NULL,
    CreatedAt       DATETIME            NOT NULL CONSTRAINT DF_Events_CreatedAt DEFAULT (GETDATE()),
    CONSTRAINT PK_Events PRIMARY KEY (EventID),
    CONSTRAINT FK_Events_Organiser FOREIGN KEY (OrganiserID)
        REFERENCES Users(UserID)
);
GO

/* =====================================================================
   3. CATEGORIES
   Belongs to a single Event. One Event -> many Categories.
   e.g. "10km Run", "21km Half Marathon", "Fun Walk 5km"
   ===================================================================== */
CREATE TABLE Categories (
    CategoryID      INT IDENTITY(1,1)   NOT NULL,
    EventID         INT                 NOT NULL,
    CategoryName    NVARCHAR(100)       NOT NULL,
    DistanceKM      DECIMAL(5,2)        NOT NULL,
    MaxParticipants INT                 NOT NULL CONSTRAINT DF_Categories_Max DEFAULT (500),
    EntryFee        DECIMAL(8,2)        NOT NULL CONSTRAINT DF_Categories_Fee DEFAULT (0.00),
    CONSTRAINT PK_Categories PRIMARY KEY (CategoryID),
    CONSTRAINT FK_Categories_Event FOREIGN KEY (EventID)
        REFERENCES Events(EventID) ON DELETE CASCADE
);
GO

/* =====================================================================
   4. ENROLMENTS
   Links a Participant (Users) to a Category. One Category -> many
   Enrolments, one Participant -> many Enrolments.
   ===================================================================== */
CREATE TABLE Enrolments (
    EnrolmentID     INT IDENTITY(1,1)   NOT NULL,
    ParticipantID   INT                 NOT NULL,
    CategoryID      INT                 NOT NULL,
    EnrolmentDate   DATETIME            NOT NULL CONSTRAINT DF_Enrolments_Date DEFAULT (GETDATE()),
    Status          VARCHAR(20)         NOT NULL
                        CONSTRAINT CK_Enrolments_Status CHECK (Status IN ('Pending','Confirmed','Cancelled'))
                        CONSTRAINT DF_Enrolments_Status DEFAULT ('Pending'),
    RaceNumber      VARCHAR(10)         NULL,
    CONSTRAINT PK_Enrolments PRIMARY KEY (EnrolmentID),
    CONSTRAINT FK_Enrolments_Participant FOREIGN KEY (ParticipantID)
        REFERENCES Users(UserID),
    CONSTRAINT FK_Enrolments_Category FOREIGN KEY (CategoryID)
        REFERENCES Categories(CategoryID),
    CONSTRAINT UQ_Enrolments_ParticipantCategory UNIQUE (ParticipantID, CategoryID)
);
GO

/* =====================================================================
   5. RESULTS
   Captured by an Organiser against a specific Enrolment. One Enrolment
   -> zero or one Result (a participant only finishes a race once).
   ===================================================================== */
CREATE TABLE Results (
    ResultID            INT IDENTITY(1,1)  NOT NULL,
    EnrolmentID         INT                NOT NULL,
    FinishTime          TIME               NOT NULL,
    OverallPosition      INT               NULL,
    CategoryPosition     INT               NULL,
    CapturedByUserID    INT                NOT NULL,
    CapturedAt          DATETIME           NOT NULL CONSTRAINT DF_Results_CapturedAt DEFAULT (GETDATE()),
    CONSTRAINT PK_Results PRIMARY KEY (ResultID),
    CONSTRAINT FK_Results_Enrolment FOREIGN KEY (EnrolmentID)
        REFERENCES Enrolments(EnrolmentID),
    CONSTRAINT FK_Results_CapturedBy FOREIGN KEY (CapturedByUserID)
        REFERENCES Users(UserID),
    CONSTRAINT UQ_Results_Enrolment UNIQUE (EnrolmentID)
);
GO

/* =====================================================================
   6. ROUTEINFO
   Route / course information for an Event, used for race-day prep.
   One Event -> many Routes (e.g. a 10km route and a 21km route).
   ===================================================================== */
CREATE TABLE RouteInfo (
    RouteID         INT IDENTITY(1,1)   NOT NULL,
    EventID         INT                 NOT NULL,
    RouteName       NVARCHAR(100)       NOT NULL,
    DistanceKM      DECIMAL(5,2)        NOT NULL,
    ElevationGainM  INT                 NULL,
    MapURL          NVARCHAR(255)       NULL,
    StartingPoint   NVARCHAR(150)       NOT NULL,
    CONSTRAINT PK_RouteInfo PRIMARY KEY (RouteID),
    CONSTRAINT FK_RouteInfo_Event FOREIGN KEY (EventID)
        REFERENCES Events(EventID) ON DELETE CASCADE
);
GO

/* =====================================================================
   SEED DATA
   ===================================================================== */

-- Organisers (2) and Participants (2+)
INSERT INTO Users (FullName, Email, PasswordHash, Role, PhoneNumber) VALUES
('Thandiwe Nkosi',   'thandiwe.nkosi@raceday.co.za',  'HASHED_PWD_1', 'Organiser',   '0821234567'),
('Johan van der Merwe','johan.vdm@raceday.co.za',     'HASHED_PWD_2', 'Organiser',   '0837654321'),
('Sipho Dlamini',     'sipho.dlamini@gmail.com',      'HASHED_PWD_3', 'Participant', '0731112222'),
('Amy Naidoo',        'amy.naidoo@gmail.com',         'HASHED_PWD_4', 'Participant', '0742223333'),
('Kagiso Molefe',     'kagiso.molefe@gmail.com',      'HASHED_PWD_5', 'Participant', '0763334444');
GO

-- Events (3), created by the two Organisers
INSERT INTO Events (OrganiserID, EventName, Description, EventType, EventDate, Location, StartTime) VALUES
(1, 'Durban Beachfront Fun Run', 'A scenic run along the Durban Golden Mile promenade.', 'Run',  '2026-10-10', 'Durban, KwaZulu-Natal', '06:00:00'),
(2, 'Cape Winelands Cycle Tour', 'A challenging cycling route through the Cape Winelands.', 'Cycle', '2026-11-14', 'Stellenbosch, Western Cape', '07:00:00'),
(1, 'Joburg Charity Park Walk', 'A family-friendly charity walk in support of local schools.', 'Walk', '2026-09-20', 'Johannesburg, Gauteng', '08:00:00');
GO

-- Categories for each Event
INSERT INTO Categories (EventID, CategoryName, DistanceKM, MaxParticipants, EntryFee) VALUES
(1, '5km Fun Run',       5.00,  500, 100.00),
(1, '10km Race',        10.00,  500, 150.00),
(2, '50km Cycle Challenge', 50.00, 300, 350.00),
(2, '100km Cycle Challenge', 100.00, 200, 450.00),
(3, '3km Charity Walk',   3.00, 1000,  50.00);
GO

-- RouteInfo for each Event
INSERT INTO RouteInfo (EventID, RouteName, DistanceKM, ElevationGainM, MapURL, StartingPoint) VALUES
(1, 'Golden Mile Route', 10.00, 15,  'https://maps.raceday.co.za/routes/golden-mile', 'North Beach, Durban'),
(2, 'Winelands Loop',    100.00, 620, 'https://maps.raceday.co.za/routes/winelands-loop', 'Stellenbosch Town Square'),
(3, 'Park Circuit',      3.00,  5,   'https://maps.raceday.co.za/routes/park-circuit', 'Zoo Lake, Johannesburg');
GO

-- Enrolments: participants enter categories (includes edge cases)
INSERT INTO Enrolments (ParticipantID, CategoryID, Status, RaceNumber) VALUES
(3, 1, 'Confirmed', 'A1001'),   -- Sipho -> 5km Fun Run
(3, 3, 'Confirmed', 'A1002'),   -- Sipho -> 50km Cycle Challenge
(4, 2, 'Confirmed', 'A1003'),   -- Amy   -> 10km Race
(5, 5, 'Pending',   NULL);      -- Kagiso -> 3km Charity Walk (not yet confirmed, no race number)
GO

-- Results captured for finished/confirmed enrolments only
INSERT INTO Results (EnrolmentID, FinishTime, OverallPosition, CategoryPosition, CapturedByUserID) VALUES
(1, '00:24:35', 12, 3, 1),  -- Sipho's 5km result, captured by Organiser Thandiwe
(3, '00:52:10', 5,  2, 1);  -- Amy's 10km result, captured by Organiser Thandiwe
GO

/* =====================================================================
   VERIFICATION QUERIES (optional - run to sanity-check the seed data)
   ===================================================================== */
-- SELECT * FROM Users;
-- SELECT * FROM Events;
-- SELECT * FROM Categories;
-- SELECT * FROM RouteInfo;
-- SELECT * FROM Enrolments;
-- SELECT * FROM Results;

PRINT 'RaceDayDB created and seeded successfully.';
GO
