using System.ComponentModel.DataAnnotations;

namespace WideWorldImporters.Domain.ValueObjects
{
    /// <summary>
    /// Value object representing packaging configuration for stock items
    /// </summary>
    public class PackagingConfiguration : IEquatable<PackagingConfiguration>
    {
        /// <summary>
        /// Usual package for selling units of this stock item
        /// </summary>
        public int UnitPackageId { get; }

        /// <summary>
        /// Usual package for selling outers of this stock item (ie cartons, boxes, etc.)
        /// </summary>
        public int OuterPackageId { get; }

        /// <summary>
        /// Quantity of the stock item in an outer package
        /// </summary>
        public int QuantityPerOuter { get; }

        /// <summary>
        /// Creates new packaging configuration
        /// </summary>
        /// <param name="unitPackageId">Unit package type ID</param>
        /// <param name="outerPackageId">Outer package type ID</param>
        /// <param name="quantityPerOuter">Quantity per outer package</param>
        public PackagingConfiguration(int unitPackageId, int outerPackageId, int quantityPerOuter)
        {
            if (unitPackageId <= 0)
                throw new ArgumentException("Unit package ID must be a valid reference.", nameof(unitPackageId));

            if (outerPackageId <= 0)
                throw new ArgumentException("Outer package ID must be a valid reference.", nameof(outerPackageId));

            if (quantityPerOuter <= 0)
                throw new ArgumentException("Quantity per outer must be greater than zero.", nameof(quantityPerOuter));

            UnitPackageId = unitPackageId;
            OuterPackageId = outerPackageId;
            QuantityPerOuter = quantityPerOuter;
        }

        /// <summary>
        /// Calculates the total number of units for a given number of outer packages
        /// </summary>
        /// <param name="outerPackages">Number of outer packages</param>
        /// <returns>Total units</returns>
        public int CalculateTotalUnits(int outerPackages)
        {
            if (outerPackages < 0)
                throw new ArgumentException("Outer packages cannot be negative.", nameof(outerPackages));

            return outerPackages * QuantityPerOuter;
        }

        /// <summary>
        /// Calculates the number of outer packages needed for a given number of units
        /// </summary>
        /// <param name="units">Number of units</param>
        /// <returns>Number of outer packages needed (rounded up)</returns>
        public int CalculateOuterPackagesNeeded(int units)
        {
            if (units < 0)
                throw new ArgumentException("Units cannot be negative.", nameof(units));

            return (int)Math.Ceiling((double)units / QuantityPerOuter);
        }

        public bool Equals(PackagingConfiguration? other)
        {
            if (other is null) return false;
            if (ReferenceEquals(this, other)) return true;
            
            return UnitPackageId == other.UnitPackageId &&
                   OuterPackageId == other.OuterPackageId &&
                   QuantityPerOuter == other.QuantityPerOuter;
        }

        public override bool Equals(object? obj)
        {
            return Equals(obj as PackagingConfiguration);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(UnitPackageId, OuterPackageId, QuantityPerOuter);
        }

        public override string ToString()
        {
            return $"Unit Package ID: {UnitPackageId}, Outer Package ID: {OuterPackageId}, {QuantityPerOuter} units per outer";
        }

        public static bool operator ==(PackagingConfiguration? left, PackagingConfiguration? right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(PackagingConfiguration? left, PackagingConfiguration? right)
        {
            return !Equals(left, right);
        }
    }
} 