using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using RaceDayPOE2.API.Data;
using RaceDayPOE2.API.Models;

namespace RaceDayPOE2.API.Services
{
    public class PasswordMigrationRunner
    {
       public static async Task RunAsync(IServiceProvider services)
        {
            await using var scope = services.CreateAsyncScope();

            var context = scope.ServiceProvider.GetRequiredService<RaceDayDbContext>();

            var participantHasher = new PasswordHasher<Participant>();
            var organizerHasher = new PasswordHasher<EventOrganizer>();

            var participants = await context.Participants
                .Where(p => p.passwordHash == null ||
                    p.passwordHash == "")
                .OrderBy(p => p.participantID)
                .ToListAsync();

            var organizers = await context.EventOrganizers
                .Where(o => o.passwordHash == null ||
                    o.passwordHash == "")
                .OrderBy(o => o.organizerID)
                .ToListAsync();

            Console.WriteLine("RaceDay password Initialization");
            Console.WriteLine("--------------------------------");
            Console.WriteLine($"Participants requiring passwords: {participants.Count}");
            Console.WriteLine($"Organizers requiring passwords: {organizers.Count}");
            Console.WriteLine("Passwords will not be displayed or printed.");
            Console.WriteLine();

            foreach (var participant in participants)
            {
                Console.WriteLine($"Participant ID: {participant.participantID}");
                Console.WriteLine($"Name: {participant.name}");

                string? password = ReadNewPassword();

                if(password == null)
                {
                    Console.WriteLine("Skipped.\n");
                    continue;
                }

                participant.passwordHash = participantHasher.HashPassword(participant, password);

                await context.SaveChangesAsync();

                Console.WriteLine("Password hash saved.\n");
            }

            foreach(var organizer in organizers)
            {
                Console.WriteLine($"Organizer ID: {organizer.organizerID}");
                Console.WriteLine($"Name: {organizer.name}");

                string? password = ReadNewPassword();

                if(password==null)
                {
                    Console.WriteLine("Skipped.\n");
                    continue;
                }

                organizer.passwordHash = organizerHasher.HashPassword(organizer, password);

                await context.SaveChangesAsync();

                Console.WriteLine("Password hash saved.\n");
            }

            Console.WriteLine("Password initialization finished.");
        }

        private static string? ReadNewPassword()
        {
            while (true)
            {
                Console.Write("Enter new password (minimum 8 characters), " +
                    "or press Enter to skip: ");

                string password = ReadHiddenPassword();

                if (string.IsNullOrEmpty(password))
                    return null;
                if(password.Length < 8)
                {
                    Console.WriteLine("Password must be at least 8 characters.");
                    continue;
                }

                Console.Write("Confirm password: ");
                string confirmation = ReadHiddenPassword();

                if(password != confirmation)
                {
                    Console.WriteLine("Passwords do not match.");
                    continue;
                }

                return password;
            }
        }

        private static string ReadHiddenPassword()
        {
            var characters = new List<char>();

            while(true)
            {
                ConsoleKeyInfo key = Console.ReadKey(intercept: true);

                if(key.Key == ConsoleKey.Enter)
                {
                    Console.WriteLine();
                    return new string(characters.ToArray());
                }

                if(key.Key == ConsoleKey.Backspace)
                {
                    if(characters.Count > 0)
                        characters.RemoveAt(characters.Count - 1);

                    continue;
                }

                if (!char.IsControl(key.KeyChar))
                    characters.Add(key.KeyChar);
            }
        }
    }
}
