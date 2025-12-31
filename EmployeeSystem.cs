class Employee
{
    int id;
    string name;
    string department;
    bool working;

    public bool IsWorking(){
        return working;
    }

    public void TerminateEmployee(){
        working = false;
    }
}

interface IEmployeeStorage
{
    void SaveToDatabase(Employee employee);
}

class EmployeeDatabaseStorage : IEmployeeStorage
{
    public void SaveToDatabase(Employee employee){}
}

interface IReportGenerator
{
    void GenerateReport(Employee employee);
}

class XmlReportGenerator : IReportGenerator
{
    public void GenerateReport(Employee employee){}
}

class CsvReportGenerator : IReportGenerator
{
    public void GenerateReport(Employee employee){}
}