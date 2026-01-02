Feature: Complex Migration Scenarios
    As a developer
    I want to perform complex migration operations
    So that I can handle advanced database evolution scenarios

Background:
    Given a MongoDB database with clients collection
    And the collection has the following documents
        | name | age |
        | Alex | 17  |
        | Max  | 25  |

Scenario: Saw-like migration - down and then up
    Given the database is at version "1.1.0"
    When I run migration down to version "1.0.0"
    Then the migration should succeed
    And the database version should be "1.0.0"
    When I run migration to latest version
    Then the migration should succeed

Scenario: Rollback by steps
    Given the database is at version "1.1.0"
    When I rollback 1 migration step
    Then the migration should succeed
    And the database version should be "1.0.0"

Scenario: Dry run does not apply changes
    When I run dry run migration to version "1.0.0"
    Then the migration should succeed
    And the result should indicate dry run
    And the documents should still have "name" field

Scenario: Migration hooks are invoked
    When I run migration to version "1.0.0" with hooks
    Then the migration should succeed
    And the before hook should be called 1 time
    And the after hook should be called 1 time

Scenario: Multiple rollbacks in sequence
    Given the database is at version "1.1.0"
    When I rollback 2 migration steps
    Then the migration should succeed
    And the database version should be "0.0.0"

Scenario: Cancellation token is respected
    When I run migration to version "1.0.0" with cancellation
    Then the migration should be cancelled
