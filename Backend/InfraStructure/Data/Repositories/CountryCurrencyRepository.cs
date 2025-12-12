using Backend.Application.Dtos;
using Backend.Domain.Entities;
using Backend.Domain.Interfaces;
using Backend.InfraStructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Backend.Infrastructure.Data.Repositories;

public class CountryCurrencyRepository : ICountryCurrencyRepository
    {
    private readonly AppDbContext _context;

    public CountryCurrencyRepository(AppDbContext context)
        {
        _context = context;
        }

    public async Task<List<CountryCurrency>> GetAllAsync()
        {
        var data = await _context.Currencies
            .OrderBy(p => p.Country)
            .ToListAsync();

        return data;
        }

    public async Task CreateOrUpdateAllAsync(List<CountryCurrencyDto> currencies)
        {
        // I can do it on memory because the countries list is small (there are less than 300 countries in the world)
        var existingRecords = await _context.Currencies.ToListAsync();

        // Create a dictionary of existing records by (Country, Currency) composite key
        var existingDict = existingRecords
            .ToDictionary(e => (e.Country, e.Currency));

        // Create a set of (Country, Currency) pairs from input for comparison
        var inputPairs = currencies
            .Select(c => (c.Country, c.Currency))
            .ToHashSet();

        // Identify records to delete (exist in DB but not in input list)
        var recordsToDelete = existingRecords
            .Where(e => !inputPairs.Contains((e.Country, e.Currency)))
            .ToList();

        foreach (var currencyDto in currencies)
            {
            var key = (currencyDto.Country, currencyDto.Currency);

            // Only add if it doesn't exist (no updates needed since Country + Currency is the key)
            if (!existingDict.ContainsKey(key))
                {
                var newRecord = new CountryCurrency
                    {
                    Country = currencyDto.Country,
                    Currency = currencyDto.Currency
                    };
                await _context.Currencies.AddAsync(newRecord);
                }
            }

        // Delete non-existent records (were removed from source list)
        if (recordsToDelete.Count != 0)
            _context.Currencies.RemoveRange(recordsToDelete);

        // Save all changes
        await _context.SaveChangesAsync();
        }
    }