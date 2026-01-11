import React, { useState, useEffect } from 'react';
import { useTasks } from '../context/TaskContext';
import { FiCheckSquare, FiClock, FiCheckCircle, FiTrendingUp, FiList, FiMenu } from 'react-icons/fi';
import {
  DndContext,
  DragOverlay,
  closestCenter,
  KeyboardSensor,
  PointerSensor,
  useSensor,
  useSensors,
  DragStartEvent,
  DragEndEvent,
  DragOverEvent,
  useDroppable,
} from '@dnd-kit/core';
import {
  SortableContext,
  sortableKeyboardCoordinates,
  useSortable,
  verticalListSortingStrategy,
} from '@dnd-kit/sortable';
import { CSS } from '@dnd-kit/utilities';
import { Task } from '../types';

interface DraggableTaskItemProps {
  task: Task;
}

const DraggableTaskItem: React.FC<DraggableTaskItemProps> = ({ task }) => {
  const {
    attributes,
    listeners,
    setNodeRef,
    transform,
    transition,
    isDragging,
  } = useSortable({ id: task.id });

  const style = {
    transform: CSS.Transform.toString(transform),
    transition,
    opacity: isDragging ? 0.5 : 1,
  };

  return (
    <div
      ref={setNodeRef}
      style={style}
      className={`bg-white rounded-xl shadow-sm border-2 p-4 mb-3 cursor-grab active:cursor-grabbing hover:shadow-md transition-all duration-200 ${
        isDragging ? 'border-primary-500 shadow-lg' : 'border-gray-200'
      }`}
      {...attributes}
      {...listeners}
    >
      <div className="flex items-start gap-3">
        <div className="flex-shrink-0 mt-0.5 text-gray-400 hover:text-primary-500 transition-colors">
          <FiMenu className="text-lg" />
        </div>
        <div className="flex-1 min-w-0">
          <h3 className={`font-semibold text-gray-900 mb-1 truncate ${
            task.isCompleted ? 'line-through text-gray-400' : ''
          }`}>
            {task.title}
          </h3>
          {task.description && (
            <p className="text-sm text-gray-600 line-clamp-2 mb-2">
              {task.description}
            </p>
          )}
          <div className="flex items-center gap-2 mt-2">
            <span className="text-xs text-gray-400">
              {new Date(task.createdAt).toLocaleDateString('en-US', {
                month: 'short',
                day: 'numeric',
              })}
            </span>
            {task.isCompleted && (
              <span className="px-2 py-0.5 text-xs font-medium bg-green-100 text-green-700 rounded-full">
                Completed
              </span>
            )}
          </div>
        </div>
      </div>
    </div>
  );
};

interface DroppableColumnProps {
  id: string;
  children: React.ReactNode;
  className?: string;
  isOver?: boolean;
}

const DroppableColumn: React.FC<DroppableColumnProps> = ({ id, children, className, isOver }) => {
  const { setNodeRef } = useDroppable({ id });

  return (
    <div
      ref={setNodeRef}
      className={`${className} ${
        isOver ? 'ring-4 ring-primary-400 ring-offset-2 bg-opacity-90 scale-[1.02]' : ''
      } transition-all duration-200`}
    >
      {children}
    </div>
  );
};

const Dashboard: React.FC = () => {
  const { fetchAllTasks, updateTask } = useTasks();
  const [allTasks, setAllTasks] = useState<Task[]>([]);
  const [loading, setLoading] = useState<boolean>(true);
  const [activeId, setActiveId] = useState<number | null>(null);
  const [overColumn, setOverColumn] = useState<string | null>(null);

  const sensors = useSensors(
    useSensor(PointerSensor),
    useSensor(KeyboardSensor, {
      coordinateGetter: sortableKeyboardCoordinates,
    })
  );

  useEffect(() => {
    loadAllTasks();
  }, []);

  const loadAllTasks = async (): Promise<void> => {
    setLoading(true);
    try {
      const tasks = await fetchAllTasks();
      setAllTasks(tasks);
    } catch (error) {
      console.error('Error loading tasks for dashboard:', error);
    } finally {
      setLoading(false);
    }
  };

  const activeTasks = allTasks.filter((task) => !task.isCompleted);
  const completedTasks = allTasks.filter((task) => task.isCompleted);

  const totalTasks = allTasks.length;
  const completedCount = completedTasks.length;
  const activeCount = activeTasks.length;
  const completionRate = totalTasks > 0 ? Math.round((completedCount / totalTasks) * 100) : 0;

  const stats = [
    {
      label: 'Total Tasks',
      value: totalTasks,
      icon: FiList,
      color: 'from-blue-500 to-blue-600',
      bgColor: 'bg-blue-50',
      iconColor: 'text-blue-600',
    },
    {
      label: 'Active Tasks',
      value: activeCount,
      icon: FiClock,
      color: 'from-amber-500 to-amber-600',
      bgColor: 'bg-amber-50',
      iconColor: 'text-amber-600',
    },
    {
      label: 'Completed',
      value: completedCount,
      icon: FiCheckCircle,
      color: 'from-green-500 to-green-600',
      bgColor: 'bg-green-50',
      iconColor: 'text-green-600',
    },
    {
      label: 'Completion Rate',
      value: `${completionRate}%`,
      icon: FiTrendingUp,
      color: 'from-purple-500 to-purple-600',
      bgColor: 'bg-purple-50',
      iconColor: 'text-purple-600',
    },
  ];

  const handleDragStart = (event: DragStartEvent): void => {
    setActiveId(event.active.id as number);
  };

  const handleDragOver = (event: DragOverEvent): void => {
    const { over } = event;
    if (!over) {
      setOverColumn(null);
      return;
    }

    const overId = over.id as string | number;
    if (overId === 'active-column' || overId === 'completed-column') {
      setOverColumn(overId as string);
    } else {
      // Check if over a task - determine column from that task
      const overTask = allTasks.find((t) => t.id === overId);
      if (overTask) {
        setOverColumn(overTask.isCompleted ? 'completed-column' : 'active-column');
      } else {
        setOverColumn(null);
      }
    }
  };

  const handleDragEnd = async (event: DragEndEvent): Promise<void> => {
    const { active, over } = event;
    setActiveId(null);
    setOverColumn(null);

    if (!over) return;

    const activeTask = allTasks.find((t) => t.id === active.id);
    if (!activeTask) return;

    // Check if dropped over a column or another task
    const overId = over.id as string | number;
    let targetIsCompleted = activeTask.isCompleted;

    // Check if dropped over a column
    if (overId === 'active-column') {
      targetIsCompleted = false;
    } else if (overId === 'completed-column') {
      targetIsCompleted = true;
    } else {
      // Dropped over another task - determine column from that task
      const overTask = allTasks.find((t) => t.id === overId);
      if (overTask) {
        targetIsCompleted = overTask.isCompleted;
      } else {
        // Not a valid drop target
        return;
      }
    }

    // Only update if status actually changed
    if (targetIsCompleted !== activeTask.isCompleted) {
      // Update local state optimistically
      setAllTasks((prev) =>
        prev.map((task) =>
          task.id === activeTask.id ? { ...task, isCompleted: targetIsCompleted } : task
        )
      );

      try {
        await updateTask(activeTask.id, {
          title: activeTask.title,
          description: activeTask.description || '',
          isCompleted: targetIsCompleted,
        });
        // Reload tasks to ensure consistency with backend
        await loadAllTasks();
      } catch (error) {
        console.error('Error updating task status:', error);
        // Revert on error
        await loadAllTasks();
      }
    }
  };

  const activeDraggedTask = activeId ? allTasks.find((t) => t.id === activeId) : null;

  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-3xl font-bold text-gray-900 mb-2">Dashboard</h1>
        <p className="text-gray-500">Overview of your task management</p>
      </div>

      <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-4 md:gap-6">
        {stats.map((stat, index) => {
          const Icon = stat.icon;
          return (
            <div
              key={index}
              className="bg-white rounded-xl shadow-sm border border-gray-100 p-6 hover:shadow-md transition-all duration-200 hover:-translate-y-1"
            >
              <div className="flex items-center justify-between mb-4">
                <div className={`${stat.bgColor} p-3 rounded-xl`}>
                  <Icon className={`${stat.iconColor} text-2xl`} />
                </div>
              </div>
              <div>
                <p className="text-gray-500 text-sm font-medium mb-1">{stat.label}</p>
                <p className="text-3xl font-bold text-gray-900">{stat.value}</p>
              </div>
            </div>
          );
        })}
      </div>

      <div className="bg-white rounded-xl shadow-sm border border-gray-100 p-6">
        <div className="flex items-center gap-3 mb-6">
          <div className="p-2 bg-primary-50 rounded-lg">
            <FiList className="text-primary-600 text-xl" />
          </div>
          <h2 className="text-xl font-semibold text-gray-900">Task Board</h2>
          <span className="text-sm text-gray-500">(Drag tasks to change status)</span>
        </div>

        {loading ? (
          <div className="text-center py-12">
            <div className="inline-block animate-spin rounded-full h-8 w-8 border-b-2 border-primary-600 mb-4"></div>
            <p className="text-gray-500 font-medium">Loading tasks...</p>
          </div>
        ) : allTasks.length === 0 ? (
          <div className="text-center py-12">
            <div className="w-16 h-16 bg-gray-100 rounded-full flex items-center justify-center mx-auto mb-4">
              <FiCheckSquare className="text-gray-400 text-2xl" />
            </div>
            <p className="text-gray-500 font-medium mb-1">No tasks yet</p>
            <p className="text-gray-400 text-sm">Start by creating a task!</p>
          </div>
        ) : (
          <DndContext
            sensors={sensors}
            collisionDetection={closestCenter}
            onDragStart={handleDragStart}
            onDragOver={handleDragOver}
            onDragEnd={handleDragEnd}
          >
            <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
              {/* Active Tasks Column */}
              <div className="space-y-4">
                <div className="flex items-center gap-3">
                  <div className="p-2 bg-amber-100 rounded-lg">
                    <FiClock className="text-amber-600 text-xl" />
                  </div>
                  <h3 className="text-lg font-semibold text-amber-900">
                    Active Tasks
                  </h3>
                  <span className="px-2 py-1 text-xs font-semibold bg-amber-200 text-amber-800 rounded-full">
                    {activeTasks.length}
                  </span>
                </div>
                <DroppableColumn
                  id="active-column"
                  className="bg-amber-50 rounded-xl p-5 border-2 border-amber-200 min-h-[400px] transition-all duration-200"
                  isOver={overColumn === 'active-column'}
                >
                  <SortableContext
                    items={activeTasks.map((t) => t.id)}
                    strategy={verticalListSortingStrategy}
                  >
                    <div className="space-y-3 max-h-[500px] overflow-y-auto pr-2">
                      {activeTasks.length === 0 ? (
                        <div className="text-center py-12 text-amber-700">
                          <FiClock className="text-3xl mx-auto mb-2 opacity-50" />
                          <p className="text-sm font-medium">No active tasks</p>
                          <p className="text-xs text-amber-600 mt-1">Drag tasks here to activate them</p>
                        </div>
                      ) : (
                        activeTasks.map((task) => (
                          <DraggableTaskItem key={task.id} task={task} />
                        ))
                      )}
                    </div>
                  </SortableContext>
                </DroppableColumn>
              </div>

              {/* Completed Tasks Column */}
              <div className="space-y-4">
                <div className="flex items-center gap-3">
                  <div className="p-2 bg-green-100 rounded-lg">
                    <FiCheckCircle className="text-green-600 text-xl" />
                  </div>
                  <h3 className="text-lg font-semibold text-green-900">
                    Completed Tasks
                  </h3>
                  <span className="px-2 py-1 text-xs font-semibold bg-green-200 text-green-800 rounded-full">
                    {completedTasks.length}
                  </span>
                </div>
                <DroppableColumn
                  id="completed-column"
                  className="bg-green-50 rounded-xl p-5 border-2 border-green-200 min-h-[400px] transition-all duration-200"
                  isOver={overColumn === 'completed-column'}
                >
                  <SortableContext
                    items={completedTasks.map((t) => t.id)}
                    strategy={verticalListSortingStrategy}
                  >
                    <div className="space-y-3 max-h-[500px] overflow-y-auto pr-2">
                      {completedTasks.length === 0 ? (
                        <div className="text-center py-12 text-green-700">
                          <FiCheckCircle className="text-3xl mx-auto mb-2 opacity-50" />
                          <p className="text-sm font-medium">No completed tasks</p>
                          <p className="text-xs text-green-600 mt-1">Drag tasks here to complete them</p>
                        </div>
                      ) : (
                        completedTasks.map((task) => (
                          <DraggableTaskItem key={task.id} task={task} />
                        ))
                      )}
                    </div>
                  </SortableContext>
                </DroppableColumn>
              </div>
            </div>

            <DragOverlay>
              {activeDraggedTask ? (
                <div className="bg-white rounded-xl shadow-2xl border-2 border-primary-500 p-4 w-80 transform rotate-3">
                  <div className="flex items-start gap-3">
                    <div className="flex-shrink-0 mt-0.5 text-primary-500">
                      <FiMenu className="text-lg" />
                    </div>
                    <div className="flex-1 min-w-0">
                      <h3 className={`font-semibold text-gray-900 mb-1 truncate ${
                        activeDraggedTask.isCompleted ? 'line-through text-gray-400' : ''
                      }`}>
                        {activeDraggedTask.title}
                      </h3>
                      {activeDraggedTask.description && (
                        <p className="text-sm text-gray-600 line-clamp-2">
                          {activeDraggedTask.description}
                        </p>
                      )}
                    </div>
                  </div>
                </div>
              ) : null}
            </DragOverlay>
          </DndContext>
        )}
      </div>
    </div>
  );
};

export default Dashboard;
