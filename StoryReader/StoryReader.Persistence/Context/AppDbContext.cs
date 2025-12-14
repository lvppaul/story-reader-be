using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using StoryReader.Persistence.Entities;

namespace StoryReader.Persistence.Context;

public partial class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<category> categories { get; set; }

    public virtual DbSet<chapter> chapters { get; set; }

    public virtual DbSet<comment> comments { get; set; }

    public virtual DbSet<favorite> favorites { get; set; }

    public virtual DbSet<rating> ratings { get; set; }

    public virtual DbSet<reading_progress> reading_progresses { get; set; }

    public virtual DbSet<refresh_token> refresh_tokens { get; set; }

    public virtual DbSet<story> stories { get; set; }

    public virtual DbSet<tag> tags { get; set; }

    public virtual DbSet<user> users { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder
            .HasPostgresEnum("story_status", new[] { "ongoing", "completed", "paused" })
            .HasPostgresExtension("uuid-ossp");

        modelBuilder.Entity<category>(entity =>
        {
            entity.HasKey(e => e.id).HasName("categories_pkey");

            entity.HasIndex(e => e.name, "categories_name_key").IsUnique();

            entity.HasIndex(e => e.slug, "categories_slug_key").IsUnique();

            entity.Property(e => e.id).HasDefaultValueSql("uuid_generate_v4()");
            entity.Property(e => e.created_at).HasDefaultValueSql("now()");
            entity.Property(e => e.name).HasMaxLength(100);
            entity.Property(e => e.slug).HasMaxLength(120);
        });

        modelBuilder.Entity<chapter>(entity =>
        {
            entity.HasKey(e => e.id).HasName("chapters_pkey");

            entity.HasIndex(e => new { e.story_id, e.chapter_number }, "chapters_story_id_chapter_number_key").IsUnique();

            entity.HasIndex(e => new { e.story_id, e.chapter_number }, "idx_chapters_story");

            entity.Property(e => e.id).HasDefaultValueSql("uuid_generate_v4()");
            entity.Property(e => e.chapter_number).HasPrecision(10, 2);
            entity.Property(e => e.created_at).HasDefaultValueSql("now()");
            entity.Property(e => e.is_premium).HasDefaultValue(false);
            entity.Property(e => e.title).HasMaxLength(300);
            entity.Property(e => e.updated_at).HasDefaultValueSql("now()");

            entity.HasOne(d => d.story).WithMany(p => p.chapters)
                .HasForeignKey(d => d.story_id)
                .HasConstraintName("chapters_story_id_fkey");
        });

        modelBuilder.Entity<comment>(entity =>
        {
            entity.HasKey(e => e.id).HasName("comments_pkey");

            entity.HasIndex(e => e.story_id, "idx_comments_story");

            entity.Property(e => e.id).HasDefaultValueSql("uuid_generate_v4()");
            entity.Property(e => e.created_at).HasDefaultValueSql("now()");

            entity.HasOne(d => d.chapter).WithMany(p => p.comments)
                .HasForeignKey(d => d.chapter_id)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("comments_chapter_id_fkey");

            entity.HasOne(d => d.parent).WithMany(p => p.Inverseparent)
                .HasForeignKey(d => d.parent_id)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("comments_parent_id_fkey");

            entity.HasOne(d => d.story).WithMany(p => p.comments)
                .HasForeignKey(d => d.story_id)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("comments_story_id_fkey");

            entity.HasOne(d => d.user).WithMany(p => p.comments)
                .HasForeignKey(d => d.user_id)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("comments_user_id_fkey");
        });

        modelBuilder.Entity<favorite>(entity =>
        {
            entity.HasKey(e => new { e.user_id, e.story_id }).HasName("favorites_pkey");

            entity.HasIndex(e => e.user_id, "idx_favorites_user");

            entity.Property(e => e.created_at).HasDefaultValueSql("now()");

            entity.HasOne(d => d.story).WithMany(p => p.favorites)
                .HasForeignKey(d => d.story_id)
                .HasConstraintName("favorites_story_id_fkey");

            entity.HasOne(d => d.user).WithMany(p => p.favorites)
                .HasForeignKey(d => d.user_id)
                .HasConstraintName("favorites_user_id_fkey");
        });

        modelBuilder.Entity<rating>(entity =>
        {
            entity.HasKey(e => e.id).HasName("ratings_pkey");

            entity.HasIndex(e => e.story_id, "idx_ratings_story");

            entity.HasIndex(e => new { e.story_id, e.user_id }, "ratings_story_id_user_id_key").IsUnique();

            entity.Property(e => e.id).HasDefaultValueSql("uuid_generate_v4()");
            entity.Property(e => e.created_at).HasDefaultValueSql("now()");

            entity.HasOne(d => d.story).WithMany(p => p.ratings)
                .HasForeignKey(d => d.story_id)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("ratings_story_id_fkey");

            entity.HasOne(d => d.user).WithMany(p => p.ratings)
                .HasForeignKey(d => d.user_id)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("ratings_user_id_fkey");
        });

        modelBuilder.Entity<reading_progress>(entity =>
        {
            entity.HasKey(e => e.id).HasName("reading_progress_pkey");

            entity.ToTable("reading_progress");

            entity.HasIndex(e => e.user_id, "idx_progress_user");

            entity.HasIndex(e => new { e.user_id, e.story_id }, "reading_progress_user_id_story_id_key").IsUnique();

            entity.Property(e => e.id).HasDefaultValueSql("uuid_generate_v4()");
            entity.Property(e => e.position).HasDefaultValue(0);
            entity.Property(e => e.updated_at).HasDefaultValueSql("now()");

            entity.HasOne(d => d.chapter).WithMany(p => p.reading_progresses)
                .HasForeignKey(d => d.chapter_id)
                .HasConstraintName("reading_progress_chapter_id_fkey");

            entity.HasOne(d => d.story).WithMany(p => p.reading_progresses)
                .HasForeignKey(d => d.story_id)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("reading_progress_story_id_fkey");

            entity.HasOne(d => d.user).WithMany(p => p.reading_progresses)
                .HasForeignKey(d => d.user_id)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("reading_progress_user_id_fkey");
        });

        modelBuilder.Entity<refresh_token>(entity =>
        {
            entity.HasKey(e => e.id).HasName("refresh_tokens_pkey");

            entity.HasIndex(e => e.token, "uq_refresh_tokens_token").IsUnique();

            entity.Property(e => e.id).HasDefaultValueSql("uuid_generate_v4()");
            entity.Property(e => e.created_at).HasDefaultValueSql("now()");
            entity.Property(e => e.token).HasMaxLength(512);

            entity.HasOne(d => d.user).WithMany(p => p.refresh_tokens)
                .HasForeignKey(d => d.user_id)
                .HasConstraintName("refresh_tokens_user_id_fkey");
        });

        modelBuilder.Entity<story>(entity =>
        {
            entity.HasKey(e => e.id).HasName("stories_pkey");

            entity.HasIndex(e => e.author_id, "idx_stories_author");

            entity.HasIndex(e => e.category_id, "idx_stories_category");

            entity.HasIndex(e => e.search_vector, "idx_stories_search").HasMethod("gin");

            entity.HasIndex(e => e.slug, "stories_slug_key").IsUnique();

            entity.Property(e => e.id).HasDefaultValueSql("uuid_generate_v4()");
            entity.Property(e => e.cover_url).HasMaxLength(1000);
            entity.Property(e => e.created_at).HasDefaultValueSql("now()");
            entity.Property(e => e.is_published).HasDefaultValue(true);
            entity.Property(e => e.slug).HasMaxLength(350);
            entity.Property(e => e.status)
                .HasMaxLength(50)
                .HasDefaultValueSql("'ongoing'::character varying");
            entity.Property(e => e.title).HasMaxLength(300);
            entity.Property(e => e.updated_at).HasDefaultValueSql("now()");
            entity.Property(e => e.views).HasDefaultValue(0L);

            entity.HasOne(d => d.author).WithMany(p => p.stories)
                .HasForeignKey(d => d.author_id)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("stories_author_id_fkey");

            entity.HasOne(d => d.category).WithMany(p => p.stories)
                .HasForeignKey(d => d.category_id)
                .HasConstraintName("stories_category_id_fkey");

            entity.HasMany(d => d.tags).WithMany(p => p.stories)
                .UsingEntity<Dictionary<string, object>>(
                    "story_tag",
                    r => r.HasOne<tag>().WithMany()
                        .HasForeignKey("tag_id")
                        .HasConstraintName("story_tags_tag_id_fkey"),
                    l => l.HasOne<story>().WithMany()
                        .HasForeignKey("story_id")
                        .HasConstraintName("story_tags_story_id_fkey"),
                    j =>
                    {
                        j.HasKey("story_id", "tag_id").HasName("story_tags_pkey");
                        j.ToTable("story_tags");
                        j.HasIndex(new[] { "tag_id" }, "idx_story_tags_tag");
                    });
        });

        modelBuilder.Entity<tag>(entity =>
        {
            entity.HasKey(e => e.id).HasName("tags_pkey");

            entity.HasIndex(e => e.name, "tags_name_key").IsUnique();

            entity.HasIndex(e => e.slug, "tags_slug_key").IsUnique();

            entity.Property(e => e.id).HasDefaultValueSql("uuid_generate_v4()");
            entity.Property(e => e.name).HasMaxLength(100);
            entity.Property(e => e.slug).HasMaxLength(120);
        });

        modelBuilder.Entity<user>(entity =>
        {
            entity.HasKey(e => e.id).HasName("users_pkey");

            entity.HasIndex(e => e.normalized_email, "uq_users_normalized_email").IsUnique();

            entity.HasIndex(e => e.email, "users_email_key").IsUnique();

            entity.Property(e => e.id).HasDefaultValueSql("uuid_generate_v4()");
            entity.Property(e => e.created_at).HasDefaultValueSql("now()");
            entity.Property(e => e.display_name).HasMaxLength(100);
            entity.Property(e => e.email).HasMaxLength(255);
            entity.Property(e => e.is_active).HasDefaultValue(true);
            entity.Property(e => e.is_email_confirmed).HasDefaultValue(false);
            entity.Property(e => e.normalized_email).HasMaxLength(255);
            entity.Property(e => e.password_hash).HasMaxLength(512);
            entity.Property(e => e.updated_at).HasDefaultValueSql("now()");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
