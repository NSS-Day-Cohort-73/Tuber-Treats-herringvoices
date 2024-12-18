using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using TuberTreats.Models;
using TuberTreats.Models.DTOs;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Database
List<TuberDriver> drivers = new List<TuberDriver>
{
    new TuberDriver
    {
        Id = 1,
        Name = "Speed Racer",
        TuberDeliveries = new List<TuberOrder>(),
    },
    new TuberDriver
    {
        Id = 2,
        Name = "Captain Falcon",
        TuberDeliveries = new List<TuberOrder>(),
    },
    new TuberDriver
    {
        Id = 3,
        Name = "Vin Diesel",
        TuberDeliveries = new List<TuberOrder>(),
    },
};

List<Customer> customers = new List<Customer>
{
    new Customer
    {
        Id = 1,
        Name = "Mario Mario",
        Address = "Mushroom Kingdom",
        TuberOrders = new List<TuberOrder>(),
    },
    new Customer
    {
        Id = 2,
        Name = "Luigi Mario",
        Address = "Haunted Mansion",
        TuberOrders = new List<TuberOrder>(),
    },
    new Customer
    {
        Id = 3,
        Name = "Princess Peach",
        Address = "Peach's Castle",
        TuberOrders = new List<TuberOrder>(),
    },
    new Customer
    {
        Id = 4,
        Name = "Bowser",
        Address = "Lava Land",
        TuberOrders = new List<TuberOrder>(),
    },
    new Customer
    {
        Id = 5,
        Name = "Toadstool",
        Address = "Toad's Hut",
        TuberOrders = new List<TuberOrder>(),
    },
};

List<Topping> toppings = new List<Topping>
{
    new Topping { Id = 1, Name = "Cheese" },
    new Topping { Id = 2, Name = "Sour Cream" },
    new Topping { Id = 3, Name = "Bacon Bits" },
    new Topping { Id = 4, Name = "Chives" },
    new Topping { Id = 5, Name = "Butter" },
};

List<TuberOrder> orders = new List<TuberOrder>
{
    new TuberOrder
    {
        Id = 1,
        CustomerId = 1,
        OrderPlacedOnDate = DateTime.Now.AddHours(-4),
        TuberDriverId = 1,
        DeliveredOnDate = null,
        Toppings = new List<Topping>(),
    },
    new TuberOrder
    {
        Id = 2,
        CustomerId = 3,
        OrderPlacedOnDate = DateTime.Now.AddHours(-3),
        TuberDriverId = 2,
        DeliveredOnDate = DateTime.Now,
        Toppings = new List<Topping>(),
    },
    new TuberOrder
    {
        Id = 3,
        CustomerId = 4,
        OrderPlacedOnDate = DateTime.Now.AddHours(-1),
        TuberDriverId = null,
        DeliveredOnDate = null,
        Toppings = new List<Topping>(),
    },
};

//TuberTopping - represents toppings added to specific orders
//Has these properties: Id, TuberOrderId, ToppingId
List<TuberTopping> tuberToppings = new List<TuberTopping>
{
    new TuberTopping
    {
        Id = 1,
        TuberOrderId = 1,
        ToppingId = 1,
    },
    new TuberTopping
    {
        Id = 2,
        TuberOrderId = 1,
        ToppingId = 3,
    },
    new TuberTopping
    {
        Id = 3,
        TuberOrderId = 2,
        ToppingId = 2,
    },
    new TuberTopping
    {
        Id = 4,
        TuberOrderId = 2,
        ToppingId = 5,
    },
};

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

// Add Endpoints

//Tuber Orders Endpoints
app.MapGet(
    "/tuberorders",
    () =>
    {
        List<TuberOrderDTO> orderDTOs = orders
            .Select(o =>
            {
                // Get all TuberToppings for the current order
                List<TuberTopping> thisTuberToppings = tuberToppings
                    .Where(tt => tt.TuberOrderId == o.Id)
                    .ToList();

                // Map Toppings to ToppingDTOs
                List<ToppingDTO> thisToppings = toppings
                    .Where(t => thisTuberToppings.Any(tt => tt.ToppingId == t.Id))
                    .Select(t => new ToppingDTO { Id = t.Id, Name = t.Name })
                    .ToList();

                // Create and return a TuberOrderDTO
                return new TuberOrderDTO
                {
                    Id = o.Id,
                    CustomerId = o.CustomerId,
                    TuberDriverId = o.TuberDriverId,
                    OrderPlacedOnDate = o.OrderPlacedOnDate,
                    DeliveredOnDate = o.DeliveredOnDate,
                    Toppings = thisToppings,
                };
            })
            .ToList();

        return Results.Ok(orderDTOs);
    }
);

app.MapGet(
    "/tuberorders/{id}",
    (int id) =>
    {
        // Find the order by Id
        TuberOrder order = orders.FirstOrDefault(o => o.Id == id);

        // Handle order not found
        if (order == null)
        {
            return Results.NotFound($"Order #{id} could not be found");
        }

        // Find the customer that matches the order's CustomerId
        Customer customer = customers.FirstOrDefault(c => c.Id == order.CustomerId);

        // Handle customer not found
        if (customer == null)
        {
            return Results.NotFound(
                $"Customer #{order.CustomerId} could not be found for Order #{id}"
            );
        }

        // Find the driver that matches the order's TuberDriverId
        TuberDriver driver =
            order.TuberDriverId != null
                ? drivers.FirstOrDefault(d => d.Id == order.TuberDriverId)
                : new TuberDriver();

        List<ToppingDTO> toppingDTOs;

        if (order.Toppings == null)
        {
            // If Toppings is null, assign an empty list to toppingDTOs
            toppingDTOs = new List<ToppingDTO>();
        }
        else
        {
            // If Toppings is not null, map to ToppingDTO and convert to a list
            toppingDTOs = order
                .Toppings.Select(t => new ToppingDTO { Id = t.Id, Name = t.Name })
                .ToList();
        }

        // Map TuberOrder to TuberOrderDTO
        TuberOrderDTO orderDTO = new TuberOrderDTO
        {
            Id = order.Id,
            CustomerId = order.CustomerId,
            Customer = new CustomerDTO
            {
                Id = customer.Id,
                Name = customer.Name,
                Address = customer.Address,
            },
            TuberDriverId = order.TuberDriverId,
            TuberDriver =
                driver != null ? new TuberDriverDTO { Id = driver.Id, Name = driver.Name } : null,
            OrderPlacedOnDate = order.OrderPlacedOnDate,
            DeliveredOnDate = order.DeliveredOnDate,
            Toppings = toppingDTOs,
        };

        // Return the mapped order
        return Results.Ok(orderDTO);
    }
);

app.MapPost(
    "/tuberorders",
    (TuberOrder newOrder) =>
    {
        // Auto-generate a new ID
        int newId = orders.Any() ? orders.Max(o => o.Id) + 1 : 1;

        // Create and populate the new order
        TuberOrder createdOrder = new TuberOrder
        {
            Id = newId,
            CustomerId = newOrder.CustomerId,
            TuberDriverId = null,
            OrderPlacedOnDate = DateTime.Now,
            DeliveredOnDate = null,
            Toppings = new List<Topping>(),
        };

        // Add the new order to the list
        orders.Add(createdOrder);

        // Return the new order
        return Results.Ok(createdOrder);
    }
);
app.MapPost(
    "/tuberorders/{id}/complete",
    (int id) =>
    {
        // Find the existing order
        TuberOrder order = orders.FirstOrDefault(o => o.Id == id);
        if (order == null)
        {
            return Results.NotFound($"Order with ID {id} not found.");
        }

        // Update the DeliveredOnDate
        order.DeliveredOnDate = DateTime.Now;
        // Return the updated order
        return Results.Ok(order);
    }
);

app.MapPut(
    "/tuberorders/{id}",
    (int id, int driverId) =>
    {
        // Find the existing order
        TuberOrder existingOrder = orders.FirstOrDefault(o => o.Id == id);
        if (existingOrder == null)
        {
            return Results.NotFound($"Order with ID {id} not found.");
        }

        // Validate that the driver exists
        TuberDriver driver = drivers.FirstOrDefault(d => d.Id == driverId);
        if (driver == null)
        {
            return Results.NotFound($"Driver with ID {driverId} not found.");
        }

        // Update the TuberDriverId property
        existingOrder.TuberDriverId = driverId;

        // Return the updated order
        return Results.Ok(existingOrder);
    }
);

//Toppings Endpoints

app.MapGet(
    "/toppings",
    () =>
    {
        List<ToppingDTO> toppingDTOs = toppings
            .Select(t => new ToppingDTO { Id = t.Id, Name = t.Name })
            .ToList();

        return Results.Ok(toppingDTOs);
    }
);

app.MapGet(
    "/toppings/{id}",
    (int id) =>
    {
        ToppingDTO? toppingDTO = toppings
            .Where(t => t.Id == id)
            .Select(t => new ToppingDTO { Id = t.Id, Name = t.Name })
            .FirstOrDefault();

        return toppingDTO != null
            ? Results.Ok(toppingDTO)
            : Results.NotFound($"Topping with ID {id} not found.");
    }
);

//Tuber Driver Endpoints
app.MapGet(
    "/tuberdrivers",
    () =>
    {
        // Map each TuberDriver to a TuberDriverDTO
        List<TuberDriverDTO> driverDTOs = drivers
            .Select(driver => new TuberDriverDTO
            {
                Id = driver.Id,
                Name = driver.Name,
                TuberDeliveries = orders
                    .Where(order => order.TuberDriverId == driver.Id)
                    .Select(order =>
                    {
                        // Get all TuberToppings for the current order
                        List<TuberTopping> thisTuberToppings = tuberToppings
                            .Where(tt => tt.TuberOrderId == order.Id)
                            .ToList();

                        // Map Toppings to ToppingDTOs
                        List<ToppingDTO> thisToppings = toppings
                            .Where(t => thisTuberToppings.Any(tt => tt.ToppingId == t.Id))
                            .Select(t => new ToppingDTO { Id = t.Id, Name = t.Name })
                            .ToList();

                        // Create and return a TuberOrderDTO
                        return new TuberOrderDTO
                        {
                            Id = order.Id,
                            CustomerId = order.CustomerId,
                            TuberDriverId = order.TuberDriverId,
                            OrderPlacedOnDate = order.OrderPlacedOnDate,
                            DeliveredOnDate = order.DeliveredOnDate,
                            Toppings = thisToppings,
                        };
                    })
                    .ToList(),
            })
            .ToList();

        // Return the list of TuberDriverDTOs
        return Results.Ok(driverDTOs);
    }
);

app.MapGet(
    "/tuberdrivers/{id}",
    (int id) =>
    {
        // Find the driver by Id
        TuberDriver driver = drivers.FirstOrDefault(d => d.Id == id);

        // Handle driver not found
        if (driver == null)
        {
            return Results.NotFound($"Driver with ID {id} not found.");
        }

        // Get the driver's deliveries
        List<TuberOrderDTO> tuberDeliveries = orders
            .Where(order => order.TuberDriverId == driver.Id)
            .Select(order =>
            {
                // Get all TuberToppings for the current order
                List<TuberTopping> thisTuberToppings = tuberToppings
                    .Where(tt => tt.TuberOrderId == order.Id)
                    .ToList();

                // Map Toppings to ToppingDTOs
                List<ToppingDTO> thisToppings = toppings
                    .Where(t => thisTuberToppings.Any(tt => tt.ToppingId == t.Id))
                    .Select(t => new ToppingDTO { Id = t.Id, Name = t.Name })
                    .ToList();

                // Create and return a TuberOrderDTO
                return new TuberOrderDTO
                {
                    Id = order.Id,
                    CustomerId = order.CustomerId,
                    TuberDriverId = order.TuberDriverId,
                    OrderPlacedOnDate = order.OrderPlacedOnDate,
                    DeliveredOnDate = order.DeliveredOnDate,
                    Toppings = thisToppings,
                };
            })
            .ToList();

        // Map the driver to a TuberDriverDTO
        TuberDriverDTO driverDTO = new TuberDriverDTO
        {
            Id = driver.Id,
            Name = driver.Name,
            TuberDeliveries = tuberDeliveries,
        };

        // Return the driver with their deliveries
        return Results.Ok(driverDTO);
    }
);

//Customers Endpoints

app.MapGet(
    "/customers",
    () =>
    {
        // Map each Customer to a CustomerDTO
        List<CustomerDTO> customerDTOs = customers
            .Select(customer => new CustomerDTO
            {
                Id = customer.Id,
                Name = customer.Name,
                Address = customer.Address,
                TuberOrders = orders
                    .Where(order => order.CustomerId == customer.Id)
                    .Select(order =>
                    {
                        // Get all TuberToppings for the current order
                        List<TuberTopping> thisTuberToppings = tuberToppings
                            .Where(tt => tt.TuberOrderId == order.Id)
                            .ToList();

                        // Map Toppings to ToppingDTOs
                        List<ToppingDTO> thisToppings = toppings
                            .Where(t => thisTuberToppings.Any(tt => tt.ToppingId == t.Id))
                            .Select(t => new ToppingDTO { Id = t.Id, Name = t.Name })
                            .ToList();

                        // Create and return a TuberOrderDTO
                        return new TuberOrderDTO
                        {
                            Id = order.Id,
                            CustomerId = order.CustomerId,
                            TuberDriverId = order.TuberDriverId,
                            OrderPlacedOnDate = order.OrderPlacedOnDate,
                            DeliveredOnDate = order.DeliveredOnDate,
                            Toppings = thisToppings,
                        };
                    })
                    .ToList(),
            })
            .ToList();

        // Return the list of CustomerDTOs
        return Results.Ok(customerDTOs);
    }
);

app.MapGet(
    "/customers/{id}",
    (int id) =>
    {
        // Find the customer by Id
        Customer customer = customers.FirstOrDefault(c => c.Id == id);

        // Handle customer not found
        if (customer == null)
        {
            return Results.NotFound($"Customer with ID {id} not found.");
        }

        // Get the customer's orders
        List<TuberOrderDTO> customerOrders = orders
            .Where(order => order.CustomerId == customer.Id) // Filter orders by CustomerId
            .Select(order =>
            {
                // Get all TuberToppings for the current order
                List<TuberTopping> thisTuberToppings = tuberToppings
                    .Where(tt => tt.TuberOrderId == order.Id)
                    .ToList();

                // Map Toppings to ToppingDTOs
                List<ToppingDTO> thisToppings = toppings
                    .Where(t => thisTuberToppings.Any(tt => tt.ToppingId == t.Id))
                    .Select(t => new ToppingDTO { Id = t.Id, Name = t.Name })
                    .ToList();

                // Map TuberOrder to TuberOrderDTO
                return new TuberOrderDTO
                {
                    Id = order.Id,
                    CustomerId = order.CustomerId,
                    TuberDriverId = order.TuberDriverId,
                    OrderPlacedOnDate = order.OrderPlacedOnDate,
                    DeliveredOnDate = order.DeliveredOnDate,
                    Toppings = thisToppings,
                };
            })
            .ToList();

        // Map the customer to a CustomerDTO with their orders
        CustomerDTO customerDTO = new CustomerDTO
        {
            Id = customer.Id,
            Name = customer.Name,
            Address = customer.Address,
            TuberOrders = customerOrders,
        };

        // Return the customer with their orders
        return Results.Ok(customerDTO);
    }
);

app.MapPost(
    "/customers",
    (Customer newCustomer) =>
    {
        // Auto-generate a new ID
        int newId = customers.Any() ? customers.Max(c => c.Id) + 1 : 1;

        // Create and populate the new customer
        Customer createdCustomer = new Customer
        {
            Id = newId,
            Name = newCustomer.Name,
            Address = newCustomer.Address,
            TuberOrders = new List<TuberOrder>(),
        };

        // Add the new customer to the list
        customers.Add(createdCustomer);

        // Return the new customer as a DTO
        CustomerDTO customerDTO = new CustomerDTO
        {
            Id = createdCustomer.Id,
            Name = createdCustomer.Name,
            Address = createdCustomer.Address,
            TuberOrders = new List<TuberOrderDTO>(),
        };

        return Results.Ok(customerDTO);
    }
);

app.MapDelete(
    "/customers/{id}",
    (int id) =>
    {
        // Find the customer by Id
        Customer customer = customers.FirstOrDefault(c => c.Id == id);

        // Handle customer not found
        if (customer == null)
        {
            return Results.NotFound($"Customer with ID {id} not found.");
        }

        // Remove the customer from the list
        customers.Remove(customer);

        // Return success message
        return Results.NoContent();
    }
);

//TuberToppings Endpoints

app.MapGet(
    "/tubertoppings",
    () =>
    {
        // Map each TuberTopping to a DTO
        List<TuberToppingDTO> tuberToppingDTOs = tuberToppings
            .Select(tt => new TuberToppingDTO
            {
                Id = tt.Id,
                TuberOrderId = tt.TuberOrderId,
                ToppingId = tt.ToppingId,
            })
            .ToList();

        // Return the list of TuberToppingDTOs
        return Results.Ok(tuberToppingDTOs);
    }
);

app.MapPost(
    "/tubertoppings",
    (TuberTopping newTuberTopping) =>
    {
        // Validate that the TuberOrder exists
        TuberOrder order = orders.FirstOrDefault(o => o.Id == newTuberTopping.TuberOrderId);
        if (order == null)
        {
            return Results.NotFound($"Order with ID {newTuberTopping.TuberOrderId} not found.");
        }

        // Validate that the Topping exists
        Topping topping = toppings.FirstOrDefault(t => t.Id == newTuberTopping.ToppingId);
        if (topping == null)
        {
            return Results.NotFound($"Topping with ID {newTuberTopping.ToppingId} not found.");
        }

        // Auto-generate an Id for the new TuberTopping
        int newId = tuberToppings.Any() ? tuberToppings.Max(tt => tt.Id) + 1 : 1;

        // Create and add the new TuberTopping
        TuberTopping createdTuberTopping = new TuberTopping
        {
            Id = newId,
            TuberOrderId = newTuberTopping.TuberOrderId,
            ToppingId = newTuberTopping.ToppingId,
        };
        tuberToppings.Add(createdTuberTopping);

        // Return the new TuberTopping object
        return Results.Ok(createdTuberTopping);
    }
);

app.MapDelete(
    "/tubertoppings/{id}",
    (int id) =>
    {
        // Find the TuberTopping by Id
        TuberTopping tuberTopping = tuberToppings.FirstOrDefault(tt => tt.Id == id);
        if (tuberTopping == null)
        {
            return Results.NotFound($"TuberTopping with ID {id} not found.");
        }

        // Remove the TuberTopping from the list
        tuberToppings.Remove(tuberTopping);

        // Return a success response
        return Results.NoContent();
    }
);

app.Run();

// Don't touch or move this!
public partial class Program { }
