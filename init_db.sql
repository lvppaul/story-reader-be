-- ============================================
-- USERS TABLE (EF CORE FRIENDLY)
-- ============================================

CREATE EXTENSION IF NOT EXISTS "uuid-ossp";

DROP TABLE IF EXISTS users CASCADE;

CREATE TABLE users (
  id uuid PRIMARY KEY DEFAULT uuid_generate_v4(),
  email varchar(255) NOT NULL UNIQUE,
  normalized_email varchar(255) NOT NULL,
  password_hash varchar(512) NOT NULL,
  display_name varchar(100),
  role varchar(20) NOT NULL DEFAULT 'user',
  is_email_confirmed boolean NOT NULL DEFAULT false,
  is_active boolean NOT NULL DEFAULT true,
  created_at timestamptz NOT NULL DEFAULT now(),
  updated_at timestamptz NOT NULL DEFAULT now(),
  last_login_at timestamptz,
  CONSTRAINT chk_users_role
    CHECK (role IN ('user', 'admin'))
);

-- INDEXES
CREATE UNIQUE INDEX IF NOT EXISTS uq_users_normalized_email
ON users(normalized_email);

CREATE INDEX IF NOT EXISTS idx_users_role
ON users(role);

CREATE INDEX IF NOT EXISTS idx_users_active
ON users(is_active);

-- FUNCTION
CREATE OR REPLACE FUNCTION update_updated_at_column()
RETURNS TRIGGER AS $$
BEGIN
  NEW.updated_at = now();
  RETURN NEW;
END;
$$ LANGUAGE plpgsql;

-- TRIGGER
DROP TRIGGER IF EXISTS update_users_updated_at ON users;

CREATE TRIGGER update_users_updated_at
BEFORE UPDATE ON users
FOR EACH ROW
EXECUTE FUNCTION update_updated_at_column();
-- ============================================
-- REFRESH TOKENS
-- ============================================
CREATE TABLE refresh_tokens (
  id uuid PRIMARY KEY DEFAULT uuid_generate_v4(),
  user_id uuid NOT NULL REFERENCES users(id) ON DELETE CASCADE,
  token varchar(512) NOT NULL,
  created_at timestamptz DEFAULT now(),
  expires_at timestamptz NOT NULL,
  revoked_at timestamptz
);

CREATE UNIQUE INDEX uq_refresh_tokens_token
ON refresh_tokens(token);
 


-- ============================================
-- CATEGORIES
-- ============================================
CREATE TABLE categories (
  id uuid PRIMARY KEY DEFAULT uuid_generate_v4(),
  name varchar(100) NOT NULL UNIQUE,
  slug varchar(120) NOT NULL UNIQUE,
  created_at timestamptz DEFAULT now()
);


-- ============================================
-- TAGS
-- ============================================
CREATE TABLE tags (
  id uuid PRIMARY KEY DEFAULT uuid_generate_v4(),
  name varchar(100) NOT NULL UNIQUE,
  slug varchar(120) NOT NULL UNIQUE
);


-- ============================================
-- STORIES
-- ============================================
CREATE TABLE stories (
  id uuid PRIMARY KEY DEFAULT uuid_generate_v4(),
  author_id uuid REFERENCES users(id) ON DELETE SET NULL,
  title varchar(300) NOT NULL,
  slug varchar(350) NOT NULL UNIQUE,
  description text,
  category_id uuid REFERENCES categories(id),
  cover_url varchar(1000),
  status varchar(50) DEFAULT 'ongoing',
  is_published boolean DEFAULT true,
  views bigint DEFAULT 0,
  created_at timestamptz DEFAULT now(),
  updated_at timestamptz DEFAULT now(),
  search_vector tsvector
);

CREATE INDEX idx_stories_author ON stories(author_id);
CREATE INDEX idx_stories_category ON stories(category_id);
CREATE INDEX idx_stories_search ON stories USING GIN(search_vector);
CREATE TYPE story_status AS ENUM ('ongoing', 'completed', 'paused');


-- ============================================
-- STORY TAGS (many-to-many)
-- ============================================
CREATE TABLE story_tags (
  story_id uuid REFERENCES stories(id) ON DELETE CASCADE,
  tag_id uuid REFERENCES tags(id) ON DELETE CASCADE,
  PRIMARY KEY (story_id, tag_id)
);

CREATE INDEX idx_story_tags_tag ON story_tags(tag_id);


-- ============================================
-- CHAPTERS
-- ============================================
CREATE TABLE chapters (
  id uuid PRIMARY KEY DEFAULT uuid_generate_v4(),
  story_id uuid NOT NULL REFERENCES stories(id) ON DELETE CASCADE,
  title varchar(300),
  chapter_number numeric(10,2) NOT NULL,
  content text NOT NULL,
  word_count int,
  is_premium boolean DEFAULT false,
  created_at timestamptz DEFAULT now(),
  updated_at timestamptz DEFAULT now(),
  UNIQUE (story_id, chapter_number)
);

CREATE INDEX idx_chapters_story 
ON chapters(story_id, chapter_number);


-- ============================================
-- COMMENTS (threaded)
-- ============================================
CREATE TABLE comments (
  id uuid PRIMARY KEY DEFAULT uuid_generate_v4(),
  story_id uuid REFERENCES stories(id) ON DELETE CASCADE,
  chapter_id uuid REFERENCES chapters(id) ON DELETE CASCADE,
  user_id uuid REFERENCES users(id) ON DELETE SET NULL,
  parent_id uuid REFERENCES comments(id) ON DELETE CASCADE,
  content text NOT NULL,
  created_at timestamptz DEFAULT now()
);

CREATE INDEX idx_comments_story ON comments(story_id);


-- ============================================
-- RATINGS
-- ============================================
CREATE TABLE ratings (
  id uuid PRIMARY KEY DEFAULT uuid_generate_v4(),
  story_id uuid REFERENCES stories(id) ON DELETE CASCADE,
  user_id uuid REFERENCES users(id) ON DELETE CASCADE,
  score smallint NOT NULL CHECK (score >= 1 AND score <= 5),
  created_at timestamptz DEFAULT now(),
  UNIQUE (story_id, user_id)
);

CREATE INDEX idx_ratings_story ON ratings(story_id);


-- ============================================
-- FAVORITES / FOLLOWS
-- ============================================
CREATE TABLE favorites (
  user_id uuid REFERENCES users(id) ON DELETE CASCADE,
  story_id uuid REFERENCES stories(id) ON DELETE CASCADE,
  created_at timestamptz DEFAULT now(),
  PRIMARY KEY (user_id, story_id)
);

CREATE INDEX idx_favorites_user ON favorites(user_id);


-- ============================================
-- READING PROGRESS
-- ============================================
CREATE TABLE reading_progress (
  id uuid PRIMARY KEY DEFAULT uuid_generate_v4(),
  user_id uuid REFERENCES users(id) ON DELETE CASCADE,
  story_id uuid REFERENCES stories(id) ON DELETE CASCADE,
  chapter_id uuid REFERENCES chapters(id),
  position int DEFAULT 0,
  updated_at timestamptz DEFAULT now(),
  UNIQUE (user_id, story_id)
);

CREATE INDEX idx_progress_user ON reading_progress(user_id);

