using System;
using System.Collections.Generic;

// Abstract User Class
public abstract class User
{
    protected string userID;
    protected string name;
    protected string phoneNumber;

    public User(string userID, string name, string phoneNumber)
    {
        this.userID = userID ?? throw new ArgumentNullException(nameof(userID));
        this.name = name ?? throw new ArgumentNullException(nameof(name));
        SetPhoneNumber(phoneNumber);
    }

    public void SetPhoneNumber(string phoneNumber)
    {
        if ((phoneNumber.Length == 10 || phoneNumber.Length == 11) && long.TryParse(phoneNumber, out _))
        {
            this.phoneNumber = phoneNumber;
        }
        else
        {
            throw new ArgumentException("Invalid phone number format. Phone number must be 10 or 11 digits long.");
        }
    }

    public string Name => name;
    public string UserID => userID;

    public virtual void DisplayProfile()
    {
        Console.WriteLine($"User  ID: {userID}, Name: {name}, Phone: {phoneNumber}");
    }

    public abstract void Register();
    public abstract void Login();
}

// Rider Class
public class Rider : User
{
    private List<Trip> rideHistory;

    public Rider(string userID, string name, string phoneNumber) : base(userID, name, phoneNumber)
    {
        rideHistory = new List<Trip>();
    }

    public override void Register()
    {
        Console.WriteLine($"{name} registered as Rider.");
    }

    public override void Login()
    {
        Console.WriteLine($"{name} logged in as Rider.");
    }

    public void RequestRide(RideSharingSystem system, string startLocation, string destination)
    {
        system.RequestRide(this, startLocation, destination);
    }

    public void ViewRideHistory()
    {
        Console.WriteLine($"{name}'s Ride History:");
        if (rideHistory.Count == 0)
        {
            Console.WriteLine("No rides found.");
            return;
        }

        foreach (var trip in rideHistory)
        {
            trip.DisplayTripDetails();
        }
    }

    public void AddRideToHistory(Trip trip)
    {
        rideHistory.Add(trip);
    }
}

// Driver Class
public class Driver : User
{
    private string driverID;
    private string vehicleDetails;
    private bool isAvailable;
    private List<Trip> tripHistory;

    public Driver(string userID, string name, string phoneNumber, string driverID, string vehicleDetails) : base(userID, name, phoneNumber)
    {
        this.driverID = driverID ?? throw new ArgumentNullException(nameof(driverID));
        this.vehicleDetails = vehicleDetails ?? throw new ArgumentNullException(nameof(vehicleDetails));
        this.isAvailable = true;
        tripHistory = new List<Trip>();
    }

    public override void Register()
    {
        Console.WriteLine($"{name} registered as Driver.");
    }

    public override void Login()
    {
        Console.WriteLine($"{name} logged in as Driver.");
    }

    public void ToggleAvailability()
    {
        isAvailable = !isAvailable;
        Console.WriteLine($"{name} is now {(isAvailable ? "available" : "not available")} for rides.");
    }

    public bool AcceptRide(Trip trip, double fare)
    {
        if (!isAvailable)
        {
            Console.WriteLine($"{name} cannot accept the ride because they are not available.");
            return false;
        }

        trip.StartTrip();
        trip.SetFare(fare);
        tripHistory.Add(trip);
        Console.WriteLine($"{name} accepted Trip ID: {trip.TripID} with fare: {fare}");
        return true;
    }

    public void ViewTripHistory()
    {
        Console.WriteLine($"{name}'s Trip History:");
        if (tripHistory.Count == 0)
        {
            Console.WriteLine("No trips found.");
            return;
        }

        foreach (var trip in tripHistory)
        {
            trip.DisplayTripDetails();
        }
    }

    public bool IsAvailable => isAvailable;
}

// Trip Class
public class Trip
{
    private static int tripCounter = 0;
    private int tripID;
    private string riderName;
    private string startLocation;
    private string destination;
    private double fare;
    private string status;

    public Trip(string riderName, string startLocation, string destination)
    {
        this.tripID = ++tripCounter;
        this.riderName = riderName ?? throw new ArgumentNullException(nameof(riderName));
        this.startLocation = startLocation ?? throw new ArgumentNullException(nameof(startLocation));
        this.destination = destination ?? throw new ArgumentNullException(nameof(destination));
        this.status = "Pending";
    }

    public void StartTrip()
    {
        status = "In Progress";
        Console.WriteLine($"Trip {tripID} started from {startLocation} to {destination}.");
    }

    public void SetFare(double fare)
    {
        this.fare = fare;
    }

    public void EndTrip()
    {
        status = "Completed";
        Console.WriteLine($"Trip {tripID} completed. Fare: {fare}");
    }

    public void DisplayTripDetails()
    {
        Console.WriteLine($"Trip {tripID}: Rider - {riderName}, From - {startLocation}, To - {destination}, Fare - {fare}, Status - {status}");
    }

    public double CalculateFare()
    {
        return Math.Sqrt(Math.Pow(Math.Abs(startLocation.Length - destination.Length), 2)) * 10;
    }

    public int TripID => tripID;
    public string RiderName => riderName;
}

// RideSharingSystem Class
public class RideSharingSystem
{
    private List<Rider> registeredRiders;
    private List<Driver> registeredDrivers;
    private List<Trip> availableTrips;
    private List<Trip> completedTrips;

    public RideSharingSystem()
    {
        registeredRiders = new List<Rider>();
        registeredDrivers = new List<Driver>();
        availableTrips = new List<Trip>();
        completedTrips = new List<Trip>();
    }

    public void RegisterUser(User user)
    {
        if (user is Rider)
        {
            registeredRiders.Add((Rider)user);
        }
        else if (user is Driver)
        {
            registeredDrivers.Add((Driver)user);
        }
    }

    public void RequestRide(Rider rider, string startLocation, string destination)
    {
        Trip trip = new Trip(rider.Name, startLocation, destination);
        availableTrips.Add(trip);
        Console.WriteLine($"Ride requested from {startLocation} to {destination} by {rider.Name}.");
    }

    public void FindAvailableDrivers()
    {
        foreach (var driver in registeredDrivers)
        {
            if (driver.IsAvailable)
            {
                Console.WriteLine($"{driver.Name} is available for rides.");
            }
        }
    }

    public void AcceptRide(Driver driver, Trip trip, Rider rider)
    {
        if (!driver.IsAvailable)
        {
            Console.WriteLine($"{driver.Name} cannot accept the ride because they are not available.");
            return;
        }

        Console.Write("Enter fare for the trip: ");
        double fare;
        while (true)
        {
            if (double.TryParse(Console.ReadLine(), out fare) && fare > 0)
            {
                break;
            }
            Console.WriteLine("Invalid fare. Please enter a valid fare.");
        }

        Console.Write("Confirm trip (yes/no): ");
        string confirmation;
        while (true)
        {
            confirmation = Console.ReadLine().ToLower();
            if (confirmation == "yes" || confirmation == "no")
            {
                break;
            }
            Console.WriteLine("Invalid confirmation. Please enter 'yes' or 'no'.");
        }

        if (confirmation == "yes")
        {
            if (driver.AcceptRide(trip, fare))
            {
                availableTrips.Remove(trip);
                Console.Write("Is the trip completed? (yes/no): ");
                string tripStatus;
                while (true)
                {
                    tripStatus = Console.ReadLine().ToLower();
                    if (tripStatus == "yes" || tripStatus == "no")
                    {
                        break;
                    }
                    Console.WriteLine("Invalid trip status. Please enter 'yes' or 'no'.");
                }

                if (tripStatus == "yes")
                {
                    trip.EndTrip();
                    completedTrips.Add(trip);
                    rider.AddRideToHistory(trip);
                    driver.ViewTripHistory();
                    rider.ViewRideHistory();
                    Console.WriteLine("Trip successfully completed.");
                }
                else
                {
                    Console.WriteLine("Error: Trip not completed.");
                }
            }
        }
        else
        {
            Console.WriteLine("Trip not confirmed.");
        }
    }

    public void DisplayAllTrips()
    {
        Console.WriteLine("All Trips:");
        if (availableTrips.Count == 0)
        {
            Console.WriteLine("No trips found.");
            return;
        }

        foreach (var trip in availableTrips)
        {
            trip.DisplayTripDetails();
        }
    }

    public void DisplayCompletedTrips()
    {
        Console.WriteLine("Completed Trips:");
        if (completedTrips.Count == 0)
        {
            Console.WriteLine("No completed trips found.");
            return;
        }

        foreach (var trip in completedTrips)
        {
            trip.DisplayTripDetails();
        }
    }

    public List<Trip> AvailableTrips => availableTrips;
    public List<Rider> RegisteredRiders => registeredRiders;
    public List<Driver> RegisteredDrivers => registeredDrivers;
}

class Program
{
    static void Main(string[] args)
    {
        RideSharingSystem system = new RideSharingSystem();

        while (true)
        {
            Console.Clear();
            Console.WriteLine("Ride-Sharing System");
            Console.WriteLine("1. Register as Rider");
            Console.WriteLine("2. Register as Driver");
            Console.WriteLine("3. Login as Rider");
            Console.WriteLine("4. Login as Driver");
            Console.WriteLine("5. View Recently Completed Trips");
            Console.WriteLine("6. Exit");
            Console.Write("Choose an option: ");
            int option;
            while (true)
            {
                if (int.TryParse(Console.ReadLine(), out option) && option >= 1 && option <= 6)
                {
                    break;
                }
                Console.WriteLine("Invalid option. Please enter a number between 1 and 6.");
            }

            switch (option)
            {
                case 1:
                    RegisterRider(system);
                    break;
                case 2:
                    RegisterDriver(system);
                    break;
                case 3:
                    LoginRider(system);
                    break;
                case 4:
                    LoginDriver(system);
                    break;
                case 5:
                    system.DisplayCompletedTrips();
                    break;
                case 6:
                    return;
                default:
                    Console.WriteLine("Invalid option. Please choose a valid option.");
                    break;
            }
        }
    }

    static void RegisterRider(RideSharingSystem system)
    {
        Console.Clear();
        Console.WriteLine("Register as Rider");
        Console.Write("Enter Rider ID: ");
        string riderID = Console.ReadLine();
        Console.Write("Enter Rider Name: ");
        string riderName = Console.ReadLine();
        Console.Write("Enter Rider Phone Number: ");
        string phoneNumber = Console.ReadLine();

        Rider rider = new Rider(riderID, riderName, phoneNumber);
        system.RegisterUser(rider);
    }

    static void RegisterDriver(RideSharingSystem system)
    {
        Console.Clear();
        Console.WriteLine("Register as Driver");
        Console.Write("Enter Driver ID: ");
        string driverID = Console.ReadLine();
        Console.Write("Enter Driver Name: ");
        string driverName = Console.ReadLine();
        Console.Write("Enter Driver Phone Number: ");
        string phoneNumber = Console.ReadLine();
        Console.Write("Enter Driver ID: ");
        string driverRID = Console.ReadLine();
        Console.Write("Enter Vehicle Details: ");
        string vehicleDetails = Console.ReadLine();

        Driver driver = new Driver(driverID, driverName, phoneNumber, driverRID, vehicleDetails);
        system.RegisterUser(driver);
    }

    static void LoginRider(RideSharingSystem system)
    {
        Console.Clear();
        Console.WriteLine("Login as Rider");
        Console.Write("Enter Rider ID: ");
        string riderID = Console.ReadLine();

        foreach (var rider in system.RegisteredRiders)
        {
            if (rider.UserID == riderID)
            {
                rider.Login();
                RiderMenu(system, rider);
                return;
            }
        }

        Console.WriteLine("Rider not found. Please register first.");
    }

    static void LoginDriver(RideSharingSystem system)
    {
        Console.Clear();
        Console.WriteLine("Login as Driver");
        Console.Write("Enter Driver ID: ");
        string driverID = Console.ReadLine();

        foreach (var driver in system.RegisteredDrivers)
        {
            if (driver.UserID == driverID)
            {
                driver.Login();
                DriverMenu(system, driver);
                return;
            }
        }

        Console.WriteLine("Driver not found. Please register first.");
    }

    static void RiderMenu(RideSharingSystem system, Rider rider)
    {
        while (true)
        {
            Console.Clear();
            Console.WriteLine("Rider Menu");
            Console.WriteLine("1. Request Ride");
            Console.WriteLine("2. View Ride History");
            Console.WriteLine("3. Logout");
            Console.Write("Choose an option: ");
            int option;
            while (true)
            {
                if (int.TryParse(Console.ReadLine(), out option) && option >= 1 && option <= 3)
                {
                    break;
                }
                Console.WriteLine("Invalid option. Please enter a number between 1 and 3.");
            }

            switch (option)
            {
                case 1:
                    RequestRide(system, rider);
                    break;
                case 2:
                    rider.ViewRideHistory();
                    break;
                case 3:
                    return;
                default:
                    Console.WriteLine("Invalid option. Please choose a valid option.");
                    break;
            }
        }
    }

    static void DriverMenu(RideSharingSystem system, Driver driver)
    {
        while (true)
        {
            Console.Clear();
            Console.WriteLine("Driver Menu");
            Console.WriteLine("1. Toggle Availability");
            Console.WriteLine("2. View Trip History");
            Console.WriteLine("3. Accept Ride");
            Console.WriteLine("4. Logout");
            Console.Write("Choose an option: ");
            int option;
            while (true)
            {
                if (int.TryParse(Console.ReadLine(), out option) && option >= 1 && option <= 4)
                {
                    break;
                }
                Console.WriteLine("Invalid option. Please enter a number between 1 and 4.");
            }

            switch (option)
            {
                case 1:
                    driver.ToggleAvailability();
                    break;
                case 2:
                    driver.ViewTripHistory();
                    break;
                case 3:
                    AcceptRide(system, driver);
                    break;
                case 4:
                    return;
                default:
                    Console.WriteLine("Invalid option. Please choose a valid option.");
                    break;
            }
        }
    }

    static void RequestRide(RideSharingSystem system, Rider rider)
    {
        Console.Clear();
        Console.WriteLine("Request Ride");
        Console.Write("Enter start location: ");
        string startLocation = Console.ReadLine();
        Console.Write("Enter destination: ");
        string destination = Console.ReadLine();

        system.RequestRide(rider, startLocation, destination);
    }

    static void AcceptRide(RideSharingSystem system, Driver driver)
    {
        Console.Clear();
        Console.WriteLine("Accept Ride");
        system.DisplayAllTrips();
        Console.Write("Enter Trip ID to accept: ");
        int tripID;
        while (true)
        {
            if (int.TryParse(Console.ReadLine(), out tripID) && tripID > 0)
            {
                break;
            }
            Console.WriteLine("Invalid trip ID. Please enter a valid trip ID.");
        }

        foreach (var trip in system.AvailableTrips)
        {
            if (trip.TripID == tripID)
            {
                foreach (var rider in system.RegisteredRiders)
                {
                    if (trip.RiderName == rider.Name)
                    {
                        system.AcceptRide(driver, trip, rider);
                        return;
                    }
                }
            }
        }

        Console.WriteLine("Trip not found. Please try again.");
    }
}