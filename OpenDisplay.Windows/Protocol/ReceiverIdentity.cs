using System.Text;

namespace OpenDisplay.Windows.Protocol;

public class ReceiverIdentity
{
    private readonly string _filePath;

    public string Id { get; }

    public ReceiverIdentity()
    {
        var directory = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData
            ),
            "OpenDisplay"
        );

        Directory.CreateDirectory(directory);

        _filePath = Path.Combine(directory, "device-id");

        Id = LoadOrCreateId();
    }

    private string LoadOrCreateId()
    {
        if (File.Exists(_filePath))
        {
            var existingId = File.ReadAllText(_filePath).Trim();

            if (Guid.TryParse(existingId, out _))
            {
                return existingId;
            }
        }

        var newId = Guid.NewGuid().ToString();

        File.WriteAllText(_filePath, newId, Encoding.UTF8);
        
        return newId;
    }
}