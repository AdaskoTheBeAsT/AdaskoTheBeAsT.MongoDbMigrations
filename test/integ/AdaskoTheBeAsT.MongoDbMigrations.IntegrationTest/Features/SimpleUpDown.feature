Feature: Simple Up Down Migration
    As a developer
    I want to run migrations up and down
    So that I can manage database versions

Background:
    Given a MongoDB database with clients collection
    And the collection has the following documents
        | name | age |
        | Alex | 17  |
        | Max  | 25  |

Scenario Outline: Migrate database to specific versions
    When I run migration to version "<version>"
    Then the migration should succeed
    And the database version should be "<version>"
    And the interim steps count should be greater than 0

Examples:
    | version |
    | 1.0.0   |
    | 1.1.0   |

Scenario Outline: Migrate with progress handling
    When I run migration to version "<version>" with progress handler
    Then the migration should succeed
    And the database version should be "<version>"
    And the progress handler should be called for each step

Examples:
    | version |
    | 1.0.0   |
    | 1.1.0   |

Scenario: Migration not found throws exception
    When I run migration to version "99.99.99"
    Then a MigrationNotFoundException should be thrown
