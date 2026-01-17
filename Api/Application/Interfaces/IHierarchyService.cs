using Api.Application.DTOs;
using Api.Core.Entities;

namespace Api.Application.Interfaces;

public interface IHierarchyService
{
    Task<Hierarchy> CreateNodeAsync(CreateNodeRequest req);
    Task<List<Hierarchy>> GetFullHierarchyAsync();
}