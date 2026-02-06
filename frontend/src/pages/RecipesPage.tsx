import { useMemo, useState, type FormEvent } from 'react';
import { RecipesApi } from '../api/recipes';
import { DishType } from '../api/models';
import type { GenerateRecipesRequestModel, RecipeModel, IngredientModel, ConfirmRecipesResponseModel, Guid } from '../api/models';
import { useAuth } from '../contexts';

type GenerateState =
  | { status: 'idle' }
  | { status: 'loading' }
  | { status: 'error'; message: string }
  | { status: 'success'; recipes: RecipeModel[] };

type ConfirmState =
  | { status: 'idle' }
  | { status: 'confirming' }
  | { status: 'error'; message: string }
  | { status: 'success'; response: ConfirmRecipesResponseModel };

const dishTypeLabels: Record<DishType, string> = {
  [DishType.Undefined]: 'Select dish type…',
  [DishType.Salad]: 'Salad',
  [DishType.Soup]: 'Soup',
  [DishType.MainCourse]: 'Main Course',
  [DishType.Dessert]: 'Dessert',
  [DishType.Appetizer]: 'Appetizer',
};

export default function RecipesPage() {
  const { apiClient } = useAuth();
  const api = useMemo(() => new RecipesApi(apiClient), [apiClient]);

  const [form, setForm] = useState<GenerateRecipesRequestModel>({
    dishType: DishType.Undefined,
    numberOfPersons: 2,
  });
  const [state, setState] = useState<GenerateState>({ status: 'idle' });
  const [selectedRecipeId, setSelectedRecipeId] = useState<Guid | null>(null);
  const [confirmState, setConfirmState] = useState<ConfirmState>({ status: 'idle' });

  async function onSubmit(e: FormEvent) {
    e.preventDefault();

    if (form.dishType === DishType.Undefined) {
      setState({ status: 'error', message: 'Please select a dish type.' });
      return;
    }

    if (form.numberOfPersons < 1 || form.numberOfPersons > 100) {
      setState({ status: 'error', message: 'Number of persons must be between 1 and 100.' });
      return;
    }

    setState({ status: 'loading' });

    try {
      const recipes = await api.generate(form);
      setState({ status: 'success', recipes });
    } catch (error: unknown) {
      const message = error instanceof Error ? error.message : 'Failed to generate recipes.';
      setState({ status: 'error', message });
    }
  }

  function onReset() {
    setForm({ dishType: DishType.Undefined, numberOfPersons: 2 });
    setState({ status: 'idle' });
    setSelectedRecipeId(null);
    setConfirmState({ status: 'idle' });
  }

  async function onConfirmCooking() {
    if (!selectedRecipeId) return;

    setConfirmState({ status: 'confirming' });

    try {
      const response = await api.confirm({ recipeIds: [selectedRecipeId] });
      if (response.success) {
        setConfirmState({ status: 'success', response });
        setSelectedRecipeId(null); // Clear selection so user can select another
      } else {
        setConfirmState({ status: 'error', message: response.message || 'Confirmation failed.' });
      }
    } catch (error: unknown) {
      const message = error instanceof Error ? error.message : 'Failed to confirm recipe.';
      setConfirmState({ status: 'error', message });
    }
  }

  function dismissSuccessMessage() {
    setConfirmState({ status: 'idle' });
  }

  const isFormError = state.status === 'error';
  const formErrorId = isFormError ? 'recipe-form-error' : undefined;

  return (
    <div className="container">
      <div className="page-header">
        <h1>Generate Recipes</h1>
        <p className="muted">
          Generate recipe suggestions based on your available inventory.
        </p>
      </div>

      {/* Generation Form - Input Section */}
      <section className="recipe-generator-section">
        <h2 className="section-title">Recipe Preferences</h2>
        <form
          onSubmit={onSubmit}
          className={`recipe-form${isFormError ? ' form--invalid' : ''}`}
          aria-invalid={isFormError}
        >
          <div className="form-row">
            <div className="form-field">
              <label htmlFor="dishType">
                Dish Type <span className="required">*</span>
              </label>
              <select
                id="dishType"
                value={form.dishType}
                onChange={(e) => setForm({ ...form, dishType: Number(e.target.value) as DishType })}
                disabled={state.status === 'loading'}
                aria-invalid={isFormError}
                aria-describedby={formErrorId}
              >
                {Object.entries(dishTypeLabels).map(([value, label]) => (
                  <option key={value} value={value} disabled={Number(value) === DishType.Undefined}>
                    {label}
                  </option>
                ))}
              </select>
            </div>

            <div className="form-field">
              <label htmlFor="numberOfPersons">
                Number of Persons <span className="required">*</span>
              </label>
              <input
                id="numberOfPersons"
                type="number"
                value={form.numberOfPersons}
                onChange={(e) => setForm({ ...form, numberOfPersons: parseInt(e.target.value) || 1 })}
                min="1"
                max="100"
                placeholder="2"
                disabled={state.status === 'loading'}
                aria-invalid={isFormError}
                aria-describedby={formErrorId}
              />
              <span className="help-text">Between 1 and 100 persons</span>
            </div>
          </div>

          <div className="form-actions">
            {state.status === 'success' && (
              <button type="button" onClick={onReset} className="secondary">
                New Search
              </button>
            )}
            <button 
              type="submit" 
              className={state.status === 'loading' ? 'loading' : ''}
              disabled={state.status === 'loading'}
            >
              {state.status === 'loading' ? 'Generating...' : 'Generate Recipes'}
            </button>
          </div>
        </form>

        {/* Error state in input section */}
        {state.status === 'error' && (
          <div id={formErrorId} className="alert-error" role="alert">
            {state.message}
          </div>
        )}
      </section>

      {/* Loading state */}
      {state.status === 'loading' && (
        <section className="recipe-loading-section">
          <div className="loading-content">
            <div className="spinner" />
            <p>Generating recipes based on your inventory...</p>
          </div>
        </section>
      )}

      {/* Results Section */}
      {state.status === 'success' && (
        <section className="recipe-results-section">
          <div className="results-header">
            <div>
              <h2 className="section-title">
                {state.recipes.length === 0 ? 'No Recipes Found' : `${state.recipes.length} Recipe${state.recipes.length > 1 ? 's' : ''} Found`}
              </h2>
              {state.recipes.length > 0 && (
                <p className="muted">
                  Select a recipe to update your inventory when you start cooking
                </p>
              )}
            </div>
          </div>

          {/* Success message after confirmation - stays visible with recipes */}
          {confirmState.status === 'success' && (
            <div className="recipe-success-banner">
              <div className="success-content">
                <div className="success-header">
                  <span className="success-icon">✓</span>
                  <strong>Recipe Confirmed!</strong>
                </div>
                <p>{confirmState.response.message}</p>
                {confirmState.response.inventoryUpdates.length > 0 && (
                  <details className="inventory-details">
                    <summary>
                      View inventory updates ({confirmState.response.inventoryUpdates.length} item{confirmState.response.inventoryUpdates.length > 1 ? 's' : ''})
                    </summary>
                    <ul className="inventory-updates">
                      {confirmState.response.inventoryUpdates.map((update, idx) => (
                        <li key={idx}>
                          <span className="update-item">{update.productName}</span>
                          <span className="update-change">
                            {update.previousQuantity} → {update.newQuantity} {update.unit}
                            <span className="used"> (-{update.usedQuantity})</span>
                          </span>
                        </li>
                      ))}
                    </ul>
                  </details>
                )}
              </div>
              <button className="btn-icon" onClick={dismissSuccessMessage} title="Dismiss" aria-label="Dismiss success message">
                ✕
              </button>
            </div>
          )}

          {/* Confirmation error */}
          {confirmState.status === 'error' && (
            <div className="alert-error" role="alert">
              {confirmState.message}
            </div>
          )}

          {state.recipes.length === 0 ? (
            <div className="empty-results">
              <p>No recipes could be generated with your current inventory.</p>
              <p className="muted">Try adding more products to your inventory or selecting a different dish type.</p>
            </div>
          ) : (
            <>
              {/* Sticky Confirmation Action Bar */}
              <div className="recipe-confirmation-bar">
                <div className="confirmation-info">
                  {selectedRecipeId ? (
                    <>
                      <span className="selected-label">Selected Recipe:</span>
                      <span className="selected-recipe">
                        {state.recipes.find(r => r.id === selectedRecipeId)?.title}
                      </span>
                    </>
                  ) : (
                    <span className="muted">Select a recipe below to confirm cooking</span>
                  )}
                </div>
                <button
                  onClick={onConfirmCooking}
                  disabled={!selectedRecipeId || confirmState.status === 'confirming'}
                  className={`confirm-btn ${confirmState.status === 'confirming' ? 'loading' : ''}`}
                >
                  {confirmState.status === 'confirming' ? 'Confirming...' : 'Confirm & Start Cooking'}
                </button>
              </div>

              {/* Recipe Cards Grid */}
              <div className="recipes-grid">
                {state.recipes.map((recipe) => (
                  <RecipeCard
                    key={recipe.id}
                    recipe={recipe}
                    isSelected={recipe.id === selectedRecipeId}
                    onSelect={() => setSelectedRecipeId(recipe.id === selectedRecipeId ? null : recipe.id)}
                    disabled={confirmState.status === 'confirming'}
                  />
                ))}
              </div>
            </>
          )}
        </section>
      )}
    </div>
  );
}

interface RecipeCardProps {
  recipe: RecipeModel;
  isSelected: boolean;
  onSelect: () => void;
  disabled: boolean;
}

function RecipeCard({ recipe, isSelected, onSelect, disabled }: RecipeCardProps) {
  return (
    <div
      className={`recipe-card ${isSelected ? 'selected' : ''} ${disabled ? 'disabled' : ''}`}
      onClick={disabled ? undefined : onSelect}
      role="button"
      tabIndex={disabled ? -1 : 0}
      aria-pressed={isSelected}
      onKeyDown={(e) => {
        if (!disabled && (e.key === 'Enter' || e.key === ' ')) {
          e.preventDefault();
          onSelect();
        }
      }}
    >
      <div className="recipe-card-header">
        <input
          type="radio"
          className="recipe-card-radio"
          checked={isSelected}
          onChange={onSelect}
          disabled={disabled}
          onClick={(e) => e.stopPropagation()}
          aria-label={`Select ${recipe.title}`}
        />
        <div>
          <h3 className="recipe-card-title">{recipe.title}</h3>
          <div className="recipe-card-meta">
            <span className="recipe-badge">{dishTypeLabels[recipe.dishType]}</span>
            <span className="recipe-badge">{recipe.ingredients.length} ingredients</span>
          </div>
        </div>
      </div>

      {recipe.description && (
        <p className="recipe-description">{recipe.description}</p>
      )}

      <div className="recipe-ingredients">
        <h4>Ingredients</h4>
        <ul>
          {recipe.ingredients.map((ingredient: IngredientModel, index: number) => (
            <li key={index}>
              {ingredient.name}: {ingredient.quantity} {ingredient.unit}
            </li>
          ))}
        </ul>
      </div>
    </div>
  );
}
