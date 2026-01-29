# PostPlate API

Backend API for generating recipes by dish name or selected ingredients. If a recipe already exists, it is retrieved from the database; otherwise, the system uses AI to generate a new recipe, stores its nutritional data, and persists any newly discovered ingredients for future use. Recipes are categorized by nutrition level (high / low calorie) and include meal suggestions for breakfast, lunch, and dinner.

## Stack
- ASP.NET Core (C#)
- SQL Server
- AWS
- JWT Authentication
- OpenAI
- Swagger

## Features
- Recipe generation with nutrition data
- Meal suggestions (breakfast, lunch, dinner)
- Nutrition-based categorization
- CRUD operations for recipes
- Search & filters
- User authentication
- Favorites
