import { useMemo, useState, type FormEvent } from 'react';
import { RecipesApi } from '../api/recipes';
import { DishType } from '../api/dtos';
import type { GenerateRecipesRequestDto, RecipeDto, IngredientDto, ConfirmRecipesResponseDto, Guid } from '../api/dtos';

type GenerateState =
  | { status: 'idle' }
  | { status: 'loading' }
  | { status: 'error'; message: string }
  | { status: 'success'; recipes: RecipeDto[] };

type ConfirmState =
  | { status: 'idle' }
  | { status: 'confirming' }
  | { status: 'error'; message: string }
  | { status: 'success'; response: ConfirmRecipesResponseDto };

const dishTypeLabels: Record<DishType, string> = {
  [DishType.Undefined]: 'Select dish type…',
  [DishType.Salad]: 'Salad',
  [DishType.Soup]: 'Soup',
  [DishType.MainCourse]: 'Main Course',
  [DishType.Dessert]: 'Dessert',
  [DishType.Appetizer]: 'Appetizer',
};

export default function RecipesPage() {
  const api = useMemo(() => new RecipesApi(), []);

  const [form, setForm] = useState<GenerateRecipesRequestDto>({
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

  return (
    <div className="container">
      <h1 className="header">Generate Recipes</h1>
      <p className="muted">
        Generate recipe suggestions based on your available inventory.
      </p>

      {/* Generation Form */}
      <form onSubmit={onSubmit} className="card" style={{ marginTop: '16px' }}>
        <div className="form-row">
          <label className="form-field">
            <span>Dish Type</span>
            <select
              value={form.dishType}
              onChange={(e) => setForm({ ...form, dishType: Number(e.target.value) as DishType })}
              disabled={state.status === 'loading'}
            >
              {Object.entries(dishTypeLabels).map(([value, label]) => (
                <option key={value} value={value} disabled={Number(value) === DishType.Undefined}>
                  {label}
                </option>
              ))}
            </select>
          </label>

          <label className="form-field">
            <span>Number of Persons</span>
            <input
              type="number"
              value={form.numberOfPersons}
              onChange={(e) => setForm({ ...form, numberOfPersons: parseInt(e.target.value) || 1 })}
              min="1"
              max="100"
              disabled={state.status === 'loading'}
            />
          </label>
        </div>

        <div className="form-actions" style={{ marginTop: '16px' }}>
          {state.status === 'success' && (
            <button type="button" onClick={onReset} className="secondary">
              Start Over
            </button>
          )}
          <button type="submit" disabled={state.status === 'loading'}>
            {state.status === 'loading' ? 'Generating…' : 'Generate Recipes'}
          </button>
        </div>
      </form>

      {/* Loading state */}
      {state.status === 'loading' && (
        <div className="loading-container">
          <div className="spinner" />
          <p className="muted">Generating recipes based on your inventory…</p>
        </div>
      )}

      {/* Error state */}
      {state.status === 'error' && (
        <p className="error" style={{ marginTop: '16px' }}>
          Error: {state.message}
        </p>
      )}

      {/* Success state - show recipes */}
      {state.status === 'success' && (
        <div className="recipes-result">
          <h2>{state.recipes.length} Recipe(s) Found</h2>

          {/* Success message after confirmation */}
          {confirmState.status === 'success' && (
            <div className="success-banner">
              <div className="success-content">
                <strong>✓ Recipe confirmed!</strong>
                <span>{confirmState.response.message}</span>
                {confirmState.response.inventoryUpdates.length > 0 && (
                  <details>
                    <summary>Inventory updated ({confirmState.response.inventoryUpdates.length} items)</summary>
                    <ul className="inventory-updates">
                      {confirmState.response.inventoryUpdates.map((update, idx) => (
                        <li key={idx}>
                          {update.productName}: {update.previousQuantity} → {update.newQuantity} {update.unit}
                          <span className="used">(-{update.usedQuantity})</span>
                        </li>
                      ))}
                    </ul>
                  </details>
                )}
              </div>
              <button className="btn-icon" onClick={dismissSuccessMessage} title="Dismiss">
                ✕
              </button>
            </div>
          )}

          {/* Confirmation error */}
          {confirmState.status === 'error' && (
            <p className="error">
              Confirmation error: {confirmState.message}
            </p>
          )}

          {state.recipes.length === 0 ? (
            <p className="muted">
              No recipes could be generated with your current inventory. Try adding more products!
            </p>
          ) : (
            <>
              <p className="muted" style={{ marginBottom: '12px' }}>
                Select a recipe and click "Confirm Cooking" to update your inventory.
              </p>

              {/* Confirm button */}
              <div className="confirm-action">
                <button
                  onClick={onConfirmCooking}
                  disabled={!selectedRecipeId || confirmState.status === 'confirming'}
                  className="confirm-btn"
                >
                  {confirmState.status === 'confirming' ? 'Confirming…' : 'Confirm Cooking'}
                </button>
                {selectedRecipeId && (
                  <span className="muted">
                    Selected: {state.recipes.find(r => r.id === selectedRecipeId)?.title}
                  </span>
                )}
              </div>

              <div className="recipes-list">
                {state.recipes.map((recipe) => (
                  <RecipeCard
                    key={recipe.id}
                    recipe={recipe}
                    isSelected={recipe.id === selectedRecipeId}
                    onSelect={() => setSelectedRecipeId(recipe.id)}
                    disabled={confirmState.status === 'confirming'}
                  />
                ))}
              </div>
            </>
          )}
        </div>
      )}
    </div>
  );
}

interface RecipeCardProps {
  recipe: RecipeDto;
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
      onKeyDown={(e) => {
        if (!disabled && (e.key === 'Enter' || e.key === ' ')) {
          e.preventDefault();
          onSelect();
        }
      }}
    >
      <div className="recipe-header">
        <div className="recipe-select">
          <input
            type="radio"
            checked={isSelected}
            onChange={onSelect}
            disabled={disabled}
            onClick={(e) => e.stopPropagation()}
          />
          <h3 className="recipe-title">{recipe.title}</h3>
        </div>
        <span className="recipe-badge">{dishTypeLabels[recipe.dishType]}</span>
      </div>

      <p className="recipe-description">{recipe.description}</p>

      <div className="recipe-ingredients">
        <h4>Ingredients</h4>
        <ul>
          {recipe.ingredients.map((ingredient: IngredientDto, index: number) => (
            <li key={index}>
              <span className="ingredient-name">{ingredient.name}</span>
              <span className="ingredient-quantity">
                {ingredient.quantity} {ingredient.unit}
              </span>
            </li>
          ))}
        </ul>
      </div>
    </div>
  );
}
