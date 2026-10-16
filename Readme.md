RACEDAY SYSTEM
PART 2 IS IN MASTER BRANCH
DESCRIPTION: 
The RaceDay system is a web-based event management system built for South African running, walking, and cycling events. The system is meant to be used by both event organizers as well as participants. Event organizers will be able to create and manage events, categories as well as participants results. Participants should be able to use the system to browse events, enter events, track personal performance history and prepare for race day using route and weather information. 

FEATURES:
The RaceDay system should allow the users to log in either as a event organizer or as a participant. Once the user has logged in, they should be able do different things depending on how they logged in. If logged in as an event organizer, the user should be able to create events and manage other events they have previously created. Organizers can view the participants who have registered and entered the event, they can monitor the results of participants after the race has concluded. If the user has logged in as a participant, they can browse upcoming events, enter into events they are interested in, and can review their personal results from past events. Participants can view the route in which the race will take as well as the weather forecast on race day. 

PROJECT STRUCTURE:
.github/workflows: check-docs.yml
  
docs: 
  API Endpoint Plan.pdf, 
  README.md, 
  RaceDay SQL Script.sql, 
  RaceDay system ERD.png, 
  Role-based access matrix.pdf, 
  User roles, 
  updated raceday system.png

Succesfull Build Screenshot PART1: 
<img width="632" height="482" alt="success workflow" src="https://github.com/user-attachments/assets/72bed10a-8b2d-4a98-a415-6dbc46f90045" />

SUCCESSFULL build screenshot PART 2:
<img width="1920" height="1008" alt="Add JWT environment variables for testing · RI-PTA-PROG6212-GR4-GR5-GR15-P1-P2-P3_prog6212-p1-p2-p3-ST10486939@4e0fcf6 - Brave 16-Oct-26 8_15_27 PM" src="https://github.com/user-attachments/assets/bcf07020-0a92-40bb-a172-072d9a99efd2" />


PROJECT SET-UP:
Before running the RaceDay API ensure that you have the follwoing installed:
- Visual Studio 2022
- .NET SDK version required (NET 8.0)
- SQL SERVER MANAGEMENT STUDIO

  USER ROLES:
  1.Participant
  A participant is a registered user that is taking part in running, walking, or cycling events.
  Participants can register an account and login, view their profile and update details, view available events and their categories, enroll in an event.

  2. Event Organizer
  An event organizer is a registered user responsible for creating and managing RaceDay events.
Organizers can:
- Register an organizer account and log in
- View their profile and update their details where supported
- Create new events
- Delete events where they have required permissions

RUN INSTRUCTIONS:
to run the RaceDay APIs:
- open the solution in Visual Studio 2022
- build the solution
- run the project
- Swagger UI should open in your browser with the API-endpoints 

PROJECT STATUS:
In Progress. 

Author: Zandile Mahlangu (ST10486939)
