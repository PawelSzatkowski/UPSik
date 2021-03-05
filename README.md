UPSik delivery company app is one of the projects I've made while doing C#/.NET Junior Developer course in Codementors, therefore I have to point out that it's currently still being developed. The solution itself contains of:
- The main, "admin" ("UPSik") app that handles adding data about users and packages to the database, as well as planning and executing the "work day";
- The notifications app ("UPSik.NotificationsSender") that prints out data when all of the packages that were to be delivered by a certain courier are actually delivered. It also sends the data via email (currently to MailSlurper);
- The web service ("UPSik.WebApi") that allows using main menu options from the admin app via http requests;
- The driver app ("UPSik.DriverClient") that connects via http to bussiness and data layers of solution's main logic, allowing the courier to log in and check his latest packing list;
- The main logic that handles connection with database, as well as calculating real distance (in straight line) between addresses and a simple algorithm that assigns packages to couriers.
