using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RestaurantSystem.Infrastructure.Persistence.EventStore.Models;

namespace RestaurantSystem.Infrastructure.Persistence.EventStore.Configurations;

public class InboxMessageConfiguration : IEntityTypeConfiguration<InboxMessage>
{
    public void Configure(EntityTypeBuilder<InboxMessage> builder)
    {
        // ۱. تعیین نام جدول و Schema
        builder.ToTable("InboxMessages", "EventStore");

        // ۲. Primary Key
        // Id اینباکس در واقع EventId یا MessageId رویداد دریافتی از Kafka است.
        // این Id باید Unique باشد تا از تکرار پردازش جلوگیری کند (Idempotency Key).
        builder.HasKey(e => e.Id);

        // ۳. ساختار فیلدها
        builder.Property(e => e.Id)
            .ValueGeneratedNever(); // چون Id از بیرون (رویداد Kafka) می‌آید، توسط EF ساخته نمی‌شود

        builder.Property(e => e.Type)
            .HasMaxLength(256)
            .IsRequired();

        builder.Property(e => e.Payload)
            .HasColumnType("jsonb") // برای ذخیره payload رویداد
            .IsRequired();

        builder.Property(e => e.OccurredOnUtc)
            .IsRequired();

        builder.Property(e => e.ProcessedAtUtc)
            .IsRequired(false); // می‌تواند null باشد تا وقتی پردازش کامل نشده است.

        builder.Property(e => e.Error)
            .IsRequired(false); // برای ثبت خطاهای احتمالی در زمان پردازش

        // نکته: Key بودن Id به طور خودکار Unique بودن آن را تضمین می‌کند و
        // همین، مکانیزم اصلی Idempotency ماست.
    }
}
