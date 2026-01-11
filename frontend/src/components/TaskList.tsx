import React from 'react';
import { useTasks } from '../context/TaskContext';
import TaskItem from './TaskItem';
import { Task } from '../types';

interface TaskListProps {
  onEdit: (task: Task) => void;
}

const TaskList: React.FC<TaskListProps> = ({ onEdit }) => {
  const { tasks } = useTasks();

  if (tasks.length === 0) {
    return (
      <div className="text-center py-16 text-gray-500 text-lg">
        <p>No tasks found. Create your first task to get started!</p>
      </div>
    );
  }

  return (
    <div className="space-y-4">
      {tasks.map((task) => (
        <TaskItem key={task.id} task={task} onEdit={onEdit} />
      ))}
    </div>
  );
};

export default TaskList;

