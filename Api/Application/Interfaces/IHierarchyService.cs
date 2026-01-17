using Api.Application.DTOs;
using Api.Core.Entities;

namespace Api.Application.Interfaces;

public interface IHierarchyService
{
    // Defines the contract for creating a node
    Task<Hierarchy> CreateNodeAsync(CreateNodeRequest request);

    // Defines the contract for fetching the tree
    Task<List<Hierarchy>> GetFullHierarchyAsync();
}