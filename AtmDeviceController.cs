public class ATMDeviceController
{
    public void Withdraw(string accountId, double amount)
    {
        try
        {
            ProcessWithdrawal(accountId, amount);
        }
        catch (Exception exception) when (
            exception is DeviceSuspendedException ||
            exception is NetworkConnectionException ||
            exception is InsufficientFundsException)
        {
            throw; 
        }
        catch (Exception exception)
        {
            throw new DeviceFailureException("Unexpected error during withdrawal", exception);
        }
    }

    private void ProcessWithdrawal(string accountId, double amount)
    {
        DeviceHandle handle = GetValidHandle();
        DeviceRecord record = RetrieveDeviceRecord(handle);

        ValidateDeviceIsActive(record);
        ValidateDeviceIsConnected(record);
        ValidateSufficientBalance(accountId, amount);

        DispenseCash(handle, amount);
    }

    private DeviceHandle GetValidHandle()
    {
        DeviceHandle handle = GetHandle(DEV1);

        if (handle == DeviceHandle.INVALID)
        {
            throw new DeviceFailureException($"Invalid Device Handle for {DEV1}");
        }

        return handle;
    }

    private void ValidateDeviceIsActive(DeviceRecord record)
    {
        if (record.Status == DEVICE_SUSPENDED)
        {
            throw new DeviceSuspendedException("Device is suspended");
        }
    }

    private void ValidateDeviceIsConnected(DeviceRecord record)
    {
        if (record.WifiConnection != WIFI_CONNECTED)
        {
            throw new NetworkConnectionException("Device is not connected to WiFi");
        }
    }

    private void ValidateSufficientBalance(string accountId, double amount)
    {
        if (GetBalance(accountId) < amount)
        {
            throw new InsufficientFundsException("Insufficient funds in account");
        }
    }
}