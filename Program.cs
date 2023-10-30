// See https://aka.ms/new-console-template for more information

Console.WriteLine("Press 1 to Enter 2 Separate Work Orders and it will grab the range in between.");
Console.WriteLine("Press 2 to enter a Single Work Order or a List separated by commas: ex 30002720,300002722,300002723,300002735 ");
Console.WriteLine("Press 3 to enter a Single PDF Part Number.");
Console.WriteLine("Press 4 to enter a Network Path of an Excel Document.");


bool validInput = false;
bool stopLoadingThread = false;
var initialStart = Console.ReadLine();

while (!validInput)
{
    if (!string.IsNullOrEmpty(initialStart))
    {
        if (int.TryParse(initialStart, out int initialRequest))
        {
            if (initialRequest == 1)
            {
                validInput = true;
            }
            else if (initialRequest == 2)
            {
                validInput = true;
            }
            else if (initialRequest == 3)
            {
                validInput = true;
            }
            else if (initialRequest == 4)
            {
                validInput = true;
            }
            else
            {
                Console.WriteLine("Incorrect Value, Please try again");
            }
        }
        else
        {
            Console.WriteLine("Incorrect Value, Please try again");
        }
    }
    else
    {
        Console.WriteLine("Incorrect Value, Please try again");
    }
}







if (validInput)
{
    Thread tAPI = null;
    Thread tLoading = new Thread(ShowLoadingSymbol);
    bool validItem = false;

    HttpClient httpClient = new HttpClient
    {
        Timeout = TimeSpan.FromSeconds(360)
    };

    try
    {
        if (initialStart.Equals("1"))
        {
            Console.WriteLine("Enter Work Order / Production Orders when the program asks for them:");
            Console.WriteLine("Please enter the first Work Order:");
            string workOrder1 = Console.ReadLine();

            Console.WriteLine("Please enter the Last Work Order in the range:");
            string workOrder2 = Console.ReadLine();

            if (!string.IsNullOrEmpty(workOrder1))
            {
                if (int.TryParse(workOrder1, out int wo1))
                {
                    if (int.TryParse(workOrder2, out int wo2))
                    {
                        validItem = true;
                    }
                }
                else
                {
                    Console.WriteLine("Incorrect Value, Please try again");
                }
            }
            else
            {
                Console.WriteLine("Incorrect Value, Please try again");
            }

            if (validItem)
            {
                Console.WriteLine($"Starting PDF Generation of Work Orders: {workOrder1} - {workOrder2}");
                tLoading.Start();

                tAPI = new Thread(() =>
                {
                    // Run API Calls in a Separate Thread
                    Task.Run(async () =>
                    {
                        await GetProductionOrdersAsync(workOrder1, workOrder2, httpClient);
                        //Thread.Sleep(3000);
                        stopLoadingThread = true;

                    }).Wait(); // Wait for Task to Complete
                });

                tAPI.Start();
                tAPI.Join();
            }
        }
        else if (initialStart.Equals("2"))
        {
            Console.WriteLine("You have selected Enter a Single Work Order or a Comma Separated List of Work Orders");
            Console.WriteLine("Please enter the Work Order(s):");
            string delimitedWorkOrders = Console.ReadLine();
            Console.WriteLine($"Starting PDF Generation of Work Orders: {delimitedWorkOrders}");
            tLoading.Start();

            tAPI = new Thread(() =>
            {
                // Run API Calls in a Separate Thread
                Task.Run(async () =>
                {
                    await GetProductionOrdersDelimitedAsync(delimitedWorkOrders, httpClient);
                    //Thread.Sleep(3000);
                    stopLoadingThread = true;

                }).Wait(); // Wait for Task to Complete
            });

            tAPI.Start();
            tAPI.Join();
        }
        else if (initialStart.Equals("3"))
        {
            Console.WriteLine("You have selected Enter a Single PDF Part Number");
            Console.WriteLine("Please enter the Part Number:");
            string pdfPartNumber = Console.ReadLine();
            Console.WriteLine($"Starting PDF Generation of Part Number: {pdfPartNumber}");
            tLoading.Start();

            tAPI = new Thread(() =>
            {
                // Run API Calls in a Separate Thread
                Task.Run(async () =>
                {
                    await GetSinglePDFAsync(pdfPartNumber, httpClient);
                    //Thread.Sleep(3000);
                    stopLoadingThread = true;

                }).Wait(); // Wait for Task to Complete
            });

            tAPI.Start();
            tAPI.Join();
        }
        else if(initialStart.Equals("4"))
        {
            Console.WriteLine("You have selected Enter by Excel Sheet: This only uses Column B for Work Orders.");
            Console.WriteLine("Please enter a Network File Path, this is the only way it works until future programming!");
            Console.WriteLine(@"\\file01\shared\foldername\filename.xlsx");
            Console.WriteLine();
            string excelFile = Console.ReadLine();
            string fileName = Path.GetFileName(excelFile);
            if (Path.Exists(excelFile))
            {
                Console.WriteLine($"Starting PDF Generation of Work Orders: {fileName}");
                tLoading.Start();

                tAPI = new Thread(() =>
                {
                    // Run API Calls in a Separate Thread
                    Task.Run(async () =>
                    {
                        await GetProductionOrdersExcelAsync(excelFile, httpClient);
                        //Thread.Sleep(3000);
                        stopLoadingThread = true;

                    }).Wait(); // Wait for Task to Complete
                });

                tAPI.Start();
                tAPI.Join();
            }

        }

        //Console.CursorVisible = true;

        // Show "Still running" until API thread is finished.
        //while (tAPI.IsAlive)
        //{
        //    Console.WriteLine("Application is running");
        //    Thread.Sleep(1000);
        //}
    }
    catch (Exception ex)
    {
        Console.WriteLine(ex.Message);
    }
}


static async Task<string> GetProductionOrdersAsync(string project1, string project2, HttpClient httpClient)
{
    using (var httpResponse = await httpClient.GetAsync($"http://10.20.55.38:5025/Production/GetProductionOrders?productionOrder={project1}&endProductionOrder={project2}"))
    //using (var httpResponse = await httpClient.GetAsync($@"http://localhost:5081/Production/GetProductionOrders?productionOrder={project1}&endProductionOrder={project2}"))
    {
        //Console.WriteLine("Inside Request");

        if (httpResponse.IsSuccessStatusCode)
        {
            Console.WriteLine();
            Console.WriteLine(await httpResponse.Content.ReadAsStringAsync());
        }
        else
        {
            Console.WriteLine();
            Console.WriteLine($"Error: {httpResponse.StatusCode} {httpResponse.RequestMessage}");
        }

        return await httpResponse.Content.ReadAsStringAsync();
    }
}

static async Task<string> GetProductionOrdersDelimitedAsync(string projectList, HttpClient httpClient)
{
    using (var httpResponse = await httpClient.GetAsync($"http://10.20.55.38:5025/Production/GetProductionOrdersDelimited?delimitedList={projectList}"))
    //using (var httpResponse = await httpClient.GetAsync($@"http://localhost:5081/Production/GetProductionOrdersDelimited?delimitedList={projectList}"))
    {
        //Console.WriteLine("Inside Request");

        if (httpResponse.IsSuccessStatusCode)
        {
            Console.WriteLine();
            Console.WriteLine(await httpResponse.Content.ReadAsStringAsync());
        }
        else
        {
            Console.WriteLine();
            Console.WriteLine($"Error: {httpResponse.StatusCode} {httpResponse.RequestMessage}");
        }

        return await httpResponse.Content.ReadAsStringAsync();
    }
}

static async Task<string> GetSinglePDFAsync(string pdfFile, HttpClient httpClient)
{
    using (var httpResponse = await httpClient.GetAsync($"http://10.20.55.38:5025/Production/GetPDFByPart?pdfPartNumber={pdfFile}"))
    //using (var httpResponse = await httpClient.GetAsync($@"http://localhost:5081/Production/GetPDFByPart?pdfPartNumber={pdfFile}"))
    {
        //Console.WriteLine("Inside Request");

        if (httpResponse.IsSuccessStatusCode)
        {
            Console.WriteLine();
            Console.WriteLine(await httpResponse.Content.ReadAsStringAsync());
        }
        else
        {
            Console.WriteLine();
            Console.WriteLine($"Error: {httpResponse.StatusCode} {httpResponse.RequestMessage}");
        }

        return await httpResponse.Content.ReadAsStringAsync();
    }
}

static async Task<string> GetProductionOrdersExcelAsync(string excelFilePath, HttpClient httpClient)
{
    using (var httpResponse = await httpClient.GetAsync($"http://10.20.55.38:5025/Production/GetProductionOrdersExcel?excelSheetPath={excelFilePath}"))
    //using (var httpResponse = await httpClient.GetAsync($@"http://localhost:5081/Production/GetProductionOrdersExcel?excelSheetPath={excelFilePath}"))
    {
        //Console.WriteLine("Inside Request");

        if (httpResponse.IsSuccessStatusCode)
        {
            Console.WriteLine();
            Console.WriteLine(await httpResponse.Content.ReadAsStringAsync());
        }
        else
        {
            Console.WriteLine();
            Console.WriteLine($"Error: {httpResponse.StatusCode} {httpResponse.RequestMessage}");
        }

        return await httpResponse.Content.ReadAsStringAsync();
    }
}

void ShowLoadingSymbol()
{
    // Define the loading symbols
    char[] loadingSymbols = { '|', '/', '-', '\\' };
    int index = 0;

    while (!stopLoadingThread)
    {
        // Move the cursor to the beginning of the line and overwrite the current line with the loading symbol
        Console.SetCursorPosition(0, Console.CursorTop);
        Console.Write($"Loading {loadingSymbols[index]}");

        // Increment the loading symbol index
        index = (index + 1) % loadingSymbols.Length;

        // Sleep for a short time to create the loading animation
        Thread.Sleep(100);
    }

    // Clear the loading symbol line when the loading is done
    Console.SetCursorPosition(0, Console.CursorTop);
    Console.Write(new string(' ', Console.BufferWidth - 1));
}

